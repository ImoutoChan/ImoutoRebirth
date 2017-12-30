var hashesResult = {};
var disableApp = false;

function requestInfo() {
    var imgs = document.getElementsByTagName("img");

    var result = new Array();

    for (var x = 0; x < imgs.length; x++) {
        var isMatched = /\/[^\s"]*[a-zA-Z0-9]{32}(\.[a-zA-Z0-9]*|\/)/ig.test(imgs[x].src);
        
        if (isMatched) {
            result.push(imgs[x]);
        }
    }
    var toSend = result.map(function (element) {
        return element.src.match(/[a-zA-Z0-9]{32}/i)[0];
    });

    var filteredToSend = toSend.filter(function(el) {
        return !Object.getOwnPropertyNames(hashesResult).some(function(e) { return e === el; });
    });

    if (filteredToSend.length > 0) {
        chrome.runtime.sendMessage({ action: "md5Try", hashes: filteredToSend });
    }
}

function saveResults(array) {
    for (var i = 0; i < array.length; i++) {
        hashesResult[array[i].md5] = array[i].result;
        console.log(array[i].md5 + " " + array[i].result);
    }
};

function updateView() {
    var result = new Array();
    var relativeResults = new Array();
    var antiresult = new Array();

    var imgs = document.getElementsByTagName("img");

    for (var p in hashesResult) {
        var regexp = new RegExp(p, "i");

        for (var x = 0; x < imgs.length; x++) {
            if (regexp.test(imgs[x].src)) {
                if (hashesResult[p] === "True" && !disableApp) {
                    result.push(imgs[x]);
                }
                else if (hashesResult[p] === "Relative" && !disableApp) {
                    relativeResults.push(imgs[x]);
                }
                else {
                    antiresult.push(imgs[x]);
                }
            }
        }
    }

    for (i = 0; i < result.length; i++) {
        result[i].classList.remove("imoutoExtNoHide");
        result[i].classList.add("imoutoExtHide");
    }

    for (i = 0; i < relativeResults.length; i++) {
        relativeResults[i].classList.remove("imoutoExtNoHide");
        relativeResults[i].classList.add("imoutoExtRelativeHide");
    }

    for (i = 0; i < antiresult.length; i++) {
        antiresult[i].classList.remove("imoutoExtHide");
        antiresult[i].classList.remove("imoutoExtRelativeHide");
        antiresult[i].classList.add("imoutoExtNoHide");
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

var observer = new MutationObserver(requestInfo);
observer.observe(document.body, {childList: true, subtree: true });
requestInfo();