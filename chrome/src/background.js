var remote = "http://localhost:60013/";
var disabledApp = false;

function log(obj) {
    console.log("Imouto Extension: " + obj.toString());
}

function sendPost(url, data, successHandler) {
    var xhr = new XMLHttpRequest();

    xhr.onload = () => successHandler(xhr.responseText);
    xhr.onerror = () => {
        log("Connection error");
    };

    xhr.open("POST", url, true);
    xhr.send(JSON.stringify(data));
}

// send md5 request to imouto server
function tryMd5(hashes, notifyTabId) {
    sendPost(remote, hashes, response => notifyTabs(JSON.parse(response), notifyTabId));
};

// send md5 results to tabs
function notifyTabs(tryMd5Response, notifyTabId) {
    chrome.tabs.sendMessage(notifyTabId, { action: "md5TryResponse", response: tryMd5Response});
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
                19: "img/grayscale/icon19g.png",
                38: "img/grayscale/icon38g.png"
            }
        });
    }
    else {
        disabledApp = false;
        chrome.browserAction.setIcon({
            path: {
                19: "img/normal/icon19.png",
                38: "img/normal/icon38.png"
            }
        });
    }

    chrome.tabs.query({},
        function(tabs) {
            for (var i = 0; i < tabs.length; ++i) {
                chrome.tabs.sendMessage(tabs[i].id, { action: "disableApp", state: disabledApp });
            };
        });
});