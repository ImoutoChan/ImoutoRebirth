import { HashResult, ImgStatus } from "../contentscript/contentscript";

export interface FoundFile {
    md5: string;
}

export interface LilinResult {
    relativesType: string;
}

const remoteRoom = "http://miyu:11301/api/CollectionFiles";
const remoteLilin = "http://miyu:11302/api/Files/relatives";
let logEnabled = false;
let disabledApp = false;

function log(obj: any): void {
    if (logEnabled) {
        console.log("Imouto Extension: " + obj.toString());
    }
}

async function getFromRoom(url: string, hashes: string[]): Promise<FoundFile[]> {
    const body = JSON.stringify({ md5: hashes });
    log("requestedHashes: " + body);
    
    const response = await fetch(url, {
        method: "POST", 
        headers: { 'Content-Type': 'application/json'}, 
        body: body
    });

    if (response.ok) {
        const responseContent = await response.json();

        log("Room result: " + JSON.stringify(responseContent));
        return responseContent;
    } else {    
        log(response.status + " " + response.statusText);
        return [];
    }
}

async function getFromLilin(url: string, hashes: string[]): Promise<Map<string, ImgStatus>> {    
    const result: Map<string, ImgStatus> = new Map<string, ImgStatus>();

    const promises = hashes.map((hash) => getSingleLilin(url, hash));

    await Promise.all(promises);

    for (const promis of promises) {
        const hashResult = await promis;
        result.set(hashResult.hash, hashResult.status);
    }

    return result;
}

async function getSingleLilin(url: string, hash: string) {
    const response = await fetch(url + '?md5=' + hash, {
        method: "GET", 
        headers: { 'Content-Type': 'application/json'}
    });

    if (response.ok) {
        const responseContent = await response.json();    
        log("Lilin result: " + JSON.stringify(responseContent));
        
        const status 
            = responseContent.length === 0 
                ? ImgStatus.None
                : ImgStatus.Relative;

        return {hash, status};

    } else {    
        log(response.status + " " + response.statusText);
        return null;
    }
}

// send md5 request to imouto server
async function tryMd5(hashes: string[], notifyTabId: number) {
    console.time('loading')

    const resultRoomPromise = getFromRoom(remoteRoom, hashes);
    const resultLilinPromise = getFromLilin(remoteLilin, hashes);

    await Promise.all([resultRoomPromise, resultLilinPromise]);

    notifyTabs(await resultRoomPromise, await resultLilinPromise, notifyTabId, hashes);

    console.timeEnd('loading')
};

// send md5 results to tabs
function notifyTabs(resultRoom: FoundFile[], resultLilin: Map<string, ImgStatus>, notifyTabId: number, hashes: string[]) {
    const results = hashes.map(x => {
        if (resultRoom.some(y => y.md5 === x)) {
            return <HashResult>{ hash: x, result: ImgStatus.Contains};
        } else {
            return <HashResult>{ hash: x, result: resultLilin.get(x)};
        }
    });

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