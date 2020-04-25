import { HashResult, ImgStatus } from "../contentscript/contentscript";

const remote = "http://miyu:11301/api/CollectionFiles";
let disabledApp = false;

function log(obj: any): void {
    console.log("Imouto Extension: " + obj.toString());
}

function sendPost(url: string, hashes: string[], successHandler: (response: string) => void) {
    const xhr = new XMLHttpRequest();

    xhr.onload = () => successHandler(xhr.responseText);
    xhr.onerror = () => log("Connection error");

    xhr.open("POST", url, true);
    xhr.setRequestHeader("Content-type", "application/json");
    xhr.send(JSON.stringify({ md5: hashes }));
}

// send md5 request to imouto server
function tryMd5(hashes: string[], notifyTabId: number) {
    sendPost(remote, hashes, response => notifyTabs(JSON.parse(response), notifyTabId));
};

// send md5 results to tabs
function notifyTabs(tryMd5Response, notifyTabId: number) {
    var results = tryMd5Response.map(x => <HashResult>{ hash: x, result: ImgStatus.Contains});

    chrome.tabs.sendMessage(notifyTabId, { action: "md5TryResponse", response: results});
};

// receive tabs messages
chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse) {
        if (request.action === "md5Try") {
            tryMd5(request.hashes, sender.tab.id);
        }
    });


// disable app
chrome.browserAction.onClicked.addListener(function (tab) {
    if (!disabledApp) {
        disabledApp = true;
        chrome.browserAction.setIcon({
            path: {
                19: "assets/img/grayscale/icon19g.png",
                38: "assets/img/grayscale/icon38g.png"
            }
        });
    } else {
        disabledApp = false;
        chrome.browserAction.setIcon({
            path: {
                19: "assets/img/normal/icon19.png",
                38: "assets/img/normal/icon38.png"
            }
        });
    }

    chrome.tabs.query({}, tabs => {
        for (const tab of tabs) {
            chrome.tabs.sendMessage(tab.id, { action: "disableApp", state: disabledApp });
        }
    });
});