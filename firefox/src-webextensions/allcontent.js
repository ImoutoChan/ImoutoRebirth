var hoverElem = null;
var forceDisableHttps = false;

function rebindImages() {
    $('img').mousemove(function (event) { hoverElem = this });
    $('a').mousemove(function (event) { hoverElem = this });
};

$(document).bind("DOMSubtreeModified", function () {
    rebindImages();
});

rebindImages();


$(document).keydown(loadImageSearch);

function loadImageSearch(e) {
    if (e.ctrlKey && (String.fromCharCode(e.which) === 'q' || String.fromCharCode(e.which) === 'Q')) {
        var res = hoverElem.outerHTML.match(/https?:\/\/[^/\s]+\/\S+\.(jpg|png|gif)/i);
        if (res != null) {
            var urlString = res[0];

            if (forceDisableHttps) {
                urlString = urlString.replace("https", "http");
            }

            chrome.runtime.sendMessage({ idCheck: "link", link: "http://iqdb.org/?url=" + encodeURIComponent(urlString) });
        } else {
            res = hoverElem.outerHTML.match(/http?:\/\/[^/\s]+\/\S+\.(jpg|png|gif)/i);

            if (res != null) {
                chrome.runtime.sendMessage({ idCheck: "link", link: "http://iqdb.org/?url=" + encodeURIComponent(res[0]) });
            }
        }
    }
};