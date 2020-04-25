import './contentscript.scss';

const hashesResult: HashResult[] = [];
const md5HashRegex = /\/[^\s"]*[a-zA-Z0-9]{32}(\.[a-zA-Z0-9]*|\/)/ig;
let disableApp = false;

export class HashResult {
    public hash: string;
    public result: ImgStatus;
}

export enum ImgStatus {
    Contains,
    Relative,
    None
}

function requestImagesInfo(): void {
    const imgsToRequest 
        = Array.from(document.getElementsByTagName("img"))
            .filter(x => md5HashRegex.test(x.src))
            .map(x => x.src.match(/[a-zA-Z0-9]{32}/i)[0])
            .filter(x => !hashesResult.map(x => x.hash).some(y => y === x));

    if (imgsToRequest.length > 0) {
        chrome.runtime.sendMessage({ action: "md5Try", hashes: imgsToRequest });
    }
}

function saveResults(results: HashResult[]): void {
    for (const result of results) {
        const existed = hashesResult.find(x => x.hash === result.hash);
        
        if (existed) {
            existed.result = result.result;
        } else {
            hashesResult.push(result);
        }

        console.log(result.hash + " " + result.result);
    }
};

function updateView(): void {
    const imgs = Array.from(document.getElementsByTagName("img"));

    for (const img of imgs) {
        const hashResult = hashesResult.find(x => new RegExp(hashResult.hash, "i").test(img.src));

        if (!hashResult) {
            continue;
        }

        img.classList.remove("imoutoExtHide");
        img.classList.remove("imoutoExtRelativeHide");
        img.classList.remove("imoutoExtNoHide");  

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

chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse) {
        if (request.action === "md5TryResponse") {
            saveResults(request.response);
        } else if (request.action === "disableApp") {
            disableApp = request.state;
        }
        updateView();
    });

var observer = new MutationObserver(requestImagesInfo);
observer.observe(document.body, {childList: true, subtree: true });
requestImagesInfo();