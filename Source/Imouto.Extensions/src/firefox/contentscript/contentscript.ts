import './contentscript.scss';
import type { FileStatusReply, KekkaiResponse } from '../../shared/fileStatus';

export class HashResult {
    public hash: string;
    public result: ImgStatus;
}

export enum ImgStatus {
    Contains,
    Relative,
    None,
    Requested
}

export interface FoundFile {
    md5: string;
}

const hashesResult: HashResult[] = [];
const pendingHashes = new Set<string>();

let logEnabled = false;
let loadingCounter: number = 0;
let localShowOwnedMediaBorders = true;

const log = (obj: any): void => {
    if (logEnabled) {
        console.log("Imouto Extension: " + obj.toString());
    }
};

const requestImagesInfo = async (): Promise<void> => {
    const { showOwnedMediaBorders } = await chrome.storage.local.get("showOwnedMediaBorders");
    localShowOwnedMediaBorders = showOwnedMediaBorders == undefined ? true : showOwnedMediaBorders;
    
    const imgsToRequest = new Set<string>();
    for (const img of Array.from(document.getElementsByTagName("img"))) {
        const hash = img.src.match(/([a-f0-9]{32})(?:\.[a-z0-9]*|\/)/i)?.[1].toLowerCase();
        if (hash && !pendingHashes.has(hash) && !hashesResult.some(result => result.hash === hash)) {
            imgsToRequest.add(hash);
        }
    }
    if (imgsToRequest.size > 0) {
        await tryMd5(Array.from(imgsToRequest));
    }
    updateView();
};

const saveResults = (results: HashResult[]): void => {
    log(JSON.stringify(results));

    for (const result of results) {
        const existed = hashesResult.find(x => x.hash === result.hash);
        
        if (existed) {
            existed.result = result.result;
        } else {
            hashesResult.push(result);
        }

        log(result.hash + " " + result.result);
    }
};

const updateView = (): void => {
    const imgs = Array.from(document.getElementsByTagName("img"));

    for (const img of imgs) {
        const hashResult = hashesResult.find(x => new RegExp(x.hash, "i").test(img.src));

        if (!hashResult) {
            continue;
        }

        img.classList.remove("imoutoExtHide");
        img.classList.remove("imoutoExtRelativeHide");
        img.classList.remove("imoutoExtNoHide");  

        if (!localShowOwnedMediaBorders) {
            continue;
        }

        switch (hashResult.result) {
            case ImgStatus.Contains:
                img.classList.add("imoutoExtHide");      
                break;
            case ImgStatus.Relative:
                img.classList.add("imoutoExtRelativeHide");  
                break;
            case ImgStatus.None:
                img.classList.add("imoutoExtNoHide");  
                break;
        }
    }
};

const getFromKekkai = async (hashes: string[], timerKey: string) => {
    hashes.forEach(hash => pendingHashes.add(hash));
    try {
        const reply: FileStatusReply = await chrome.runtime.sendMessage({ type: "getFileStatuses", hashes });
        if ("error" in reply) {
            throw new Error(reply.error);
        }
        saveResults(reply.results.map(item => ({ hash: item.hash, result: ToImgStatus(item) })));
    } catch (error) {
        log("Kekkai fail: " + String(error));
    } finally {
        hashes.forEach(hash => pendingHashes.delete(hash));
        console.timeEnd(timerKey);
    }
};

const tryMd5 = async (hashes: string[]): Promise<void> => {
    const timerKey = 'loading' + loadingCounter++;
    console.time(timerKey);
    await getFromKekkai(hashes, timerKey);
};
 
const ToImgStatus = (response: KekkaiResponse) : ImgStatus => {
    switch (response.status) {
        case "NotFound":
            return ImgStatus.None;
        case "Present":
            return ImgStatus.Contains;
        case "RelativePresent":
            return ImgStatus.Relative;
    }
};

chrome.storage.onChanged.addListener((changes, area) => {
    if (area === 'local' && changes.showOwnedMediaBorders) {
        const showOwnedMediaBorders = Boolean(changes.showOwnedMediaBorders.newValue);
        localShowOwnedMediaBorders = showOwnedMediaBorders == undefined ? true : showOwnedMediaBorders;
        updateView();
    }
});

var observer = new MutationObserver(requestImagesInfo);
observer.observe(document.body, {childList: true, subtree: true });
requestImagesInfo();