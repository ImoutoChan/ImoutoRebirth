import oboe = require("oboe");
import { HashResult, ImgStatus } from "../contentscript/contentscript";

export interface FoundFile {
    md5: string;
}

export interface KekkaiResponse {
    hash: string;
    status: "NotFound" | "Present" | "RelativePresent";
}

const token = "";

const remoteKekkai = "https://kekkai.imouto.pink/FileStatus?token=" + token;
let logEnabled = true;
let disabledApp = false;
let loadingCounter: number = 0;

function log(obj: any): void {
    if (logEnabled) {
        console.log("Imouto Extension: " + obj.toString());
    }
}

function getFromKekkai(hashes: string[], notifyTabId: number, timerKey: string) {
    const body = JSON.stringify(hashes);
    log("requestedHashes: " + body);
    
    var config = {
        url: remoteKekkai,
        method: "POST",
        cached: false,
        headers: { 'Content-Type': 'application/json'},
        body: body
    }            
      const oboeService = oboe(config);
      oboeService
        .node('!.*', (response: KekkaiResponse) => {  
            log("Kekkai result element: " + JSON.stringify(response));
            
            notifyTabs([response], notifyTabId);
        })
        .fail((errorReport: oboe.FailReason) => {
            log("Kekkai fail: " + JSON.stringify(errorReport));
            console.timeEnd(timerKey);
         })
         .done(() => {
            console.timeEnd(timerKey);
         })
}

// send md5 request to imouto server
async function tryMd5(hashes: string[], notifyTabId: number) {
    const timerKey = 'loading' + loadingCounter++;

    console.time(timerKey);

    getFromKekkai(hashes, notifyTabId, timerKey);
};
 
function ToImgStatus(response: KekkaiResponse) : ImgStatus {
    switch (response.status) {
        case "NotFound":
            return ImgStatus.None;
        case "Present":
            return ImgStatus.Contains;
        case "RelativePresent":
            return ImgStatus.Relative;
    }
}

// send md5 results to tabs
function notifyTabs(result: KekkaiResponse[], notifyTabId: number) {
    const results = result.map(x => <HashResult>{ hash: x.hash, result: ToImgStatus(x)});

    log("Notifying with: " + JSON.stringify(results));
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