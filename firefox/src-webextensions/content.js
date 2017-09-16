var hashesResult = {};
var hideEnabled = true;
$("<style type='text/css'> .imoutoExtHide{ opacity: 0.6; outline: 3px solid red; outline-offset: -3px; } .imoutoExtHide:Hover{ opacity: 1; outline: 3px solid transparent; outline-offset: -3px;} .imoutoExtRelativeHide{ opacity: 0.6; outline: 3px solid green; outline-offset: -3px; } .imoutoExtRelativeHide:Hover{ opacity: 1; outline: 3px solid transparent; outline-offset: -3px;}</style>").appendTo("head");

chrome.runtime.onMessage.addListener(function (msg, sender, sendResponse) {
    if (msg.idCheck && (msg.idCheck == "meSystem" || msg.idCheck == "me"))
    {
        if (msg.force == "enabled")
        {
            hideEnabled = true;
        }
        else
        {
            hideEnabled = false;
        }
    }

    if (msg.idCheck && (msg.idCheck == "me")) {
        var hashesProcessed = msg.md5array;

        saveResults(hashesProcessed);
    }
    updateView();
});

function saveResults(array) {
    for (var i = 0; i < array.length; i++) {
        hashesResult[array[i].md5] = array[i].result;
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
                if (hashesResult[p] == "True" && hideEnabled)
                {                    
                    result.push(imgs[x]);
                }
                else if (hashesResult[p] == "Relative" && hideEnabled)
                {
                    relativeResults.push(imgs[x]);
                }
                else
                {
                    antiresult.push(imgs[x]);
                }
            }            
        }
    }
    
    for (i = 0; i < result.length; i++) {
        $(result[i]).removeClass("imoutoExtNoHide").addClass("imoutoExtHide");
    }

    for (i = 0; i < relativeResults.length; i++) {
        $(relativeResults[i]).removeClass("imoutoExtNoHide").addClass("imoutoExtRelativeHide");
    }

    for (i = 0; i < antiresult.length; i++) {
        $(antiresult[i]).removeClass("imoutoExtHide").removeClass("imoutoExtRelativeHide").addClass("imoutoExtNoHide");
    }
};

function requestInfo()
{
    var imgs = document.getElementsByTagName("img");
    var result = new Array();
    for (var x = 0; x < imgs.length; x++) {
        if (/\/[^\s"]*[a-zA-Z0-9]{32}(\.[a-zA-Z]*|\/)/ig.test(imgs[x].src)) {
            result.push(imgs[x]);
        }
    }
    var toSend = result.map(function (element) { return element.src.match(/[a-zA-Z0-9]{32}/i)[0]; })

    var filteredToSend = toSend.filter(function (el) {
        return !Object.getOwnPropertyNames(hashesResult).some(function (e) { return e == el; });
    });

    if (filteredToSend.length > 0) {
        chrome.runtime.sendMessage({ idCheck: "me", md5sToProcess: filteredToSend });
    }
}

$(document).bind("DOMSubtreeModified", function () {
    requestInfo();
})

requestInfo();