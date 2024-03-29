import './contentscript.scss';

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

export interface KekkaiResponse {
    hash: string;
    status: "NotFound" | "Present" | "RelativePresent";
}


const hashesResult: HashResult[] = [];
const token = "";
const remoteKekkai = "http://localhost:11303/FileStatus?token=" + token;

let logEnabled = false;
let loadingCounter: number = 0;
let localShowOwnedMediaBorders = true;
let lastRequest;

const log = (obj: any): void => {
    if (logEnabled) {
        console.log("Imouto Extension: " + obj.toString());
    }
};

const requestImagesInfo = async (): Promise<void> => {
    const { showOwnedMediaBorders } = await chrome.storage.local.get("showOwnedMediaBorders");
    localShowOwnedMediaBorders = showOwnedMediaBorders == undefined ? true : showOwnedMediaBorders;
    
    const imgsToRequest 
        = Array.from(document.getElementsByTagName("img"))
            .filter(x => /\/[^\s"]*[a-zA-Z0-9]{32}(\.[a-zA-Z0-9]*|\/)/ig.test(x.src))
            .map(x => x.src.match(/[a-zA-Z0-9]{32}/i)[0])
            .filter(x => !hashesResult.map(x => x.hash).some(y => y === x));

    if (imgsToRequest.length > 0) {
        await tryMd5(imgsToRequest);
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
    if (lastRequest && lastRequest.abort) {
        lastRequest.abort();
    }
    
    const body = JSON.stringify(hashes);
    log("requestedHashes: " + body);
    
    lastRequest = new AbortController();
    const config = {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: body,
        signal: lastRequest.signal
    };

    try {
        const response = await fetch(remoteKekkai, config);
        if (!response.ok) {
            log(`HTTP error! status: ${response.status}`);
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        log("Kekkai result element: " + JSON.stringify(data));
    
        const results = data.map(x => <HashResult>{ hash: x.hash, result: ToImgStatus(x) });
        saveResults(results);
        updateView();
    } catch (error) {
        log("Kekkai fail: " + JSON.stringify(error));
    } finally {
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