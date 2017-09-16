var cachedMd5 = []; // md5, result (string: True, False, Relative)
var processingMd5 = []; // md5, tabId, result, state = { init, processing, ready }
var hideStatus = "enabled"; // enabled, disabled
var listener = "http://localhost:60024/";
var connection_state = "init"; //init, requested, connected
var mainThreadInterval;
var threadState = "init";

function sync()
{
    if (connection_state == "init")
    {
        if (threadState == "connected") {
            threadState = "init";
            clearInterval(mainThreadInterval);
            mainThreadInterval = setInterval(sync, 1000);
        }
        connect();
    }
    else 
    {
        if (threadState == "init")
        {
            threadState = "connected";
            clearInterval(mainThreadInterval);
            mainThreadInterval = setInterval(sync, 100);
        }

        //requestedReadyResponses
        receiveReadyRequest();

        //send needed
        sendToRequestArray();
    }

    notifyTabs();
}

/* -- Remote Part -- */
function connect()
{
    var postData = {
        "action": "connectRequest"
    };

    console.log("----Connect request");
    remotePost(listener, postData, function (response) {
        //var result = JSON.parse(response);
        var result = response;
        if (result == "connectAccept")
        {
            connection_state = "connected";
        }
        console.log("----Connected");
    });
}

function receiveReadyRequest() {

    if (!processingMd5.some(function (element) { return element.state == "processing"; }))
    {
        return;
    }

    var postData = {
        "action": "receiveReady"
    };

    $.post(listener, postData, function (response) {
        var result = JSON.parse (response);
        if (result.length > 0) {
            console.log("Getted " + result.length + " results from native app");

            for (var i = 0; i < result.length; i++)
            {
                for (var j = 0; j < processingMd5.length; j++)
                {
                    if (result[i].md5 == processingMd5[j].md5)
                    {
                        processingMd5[j].result = result[i].result;
                        processingMd5[j].state = "ready";
                    }
                }

                if (tryGetCached(result[i].md5) == null)
                {
                    cachedMd5.push({
                        md5: result[i].md5,
                        result: result[i].result
                    });
                }
            }
        }
    }).fail(function () {
        connection_state = "init";
        console.log("----Connection lost");
    });
};

function sendToRequestArray() {

    var sendedArray = processingMd5.filter(function (el) { return el.state == "init"}).map(function (element) {
        return {
            md5: element.md5
        }
    });

    if (sendedArray.length > 0) {

        var json = JSON.stringify(sendedArray);

        send("sendToRequest", json);

        changeProcessingState(sendedArray, "processing");

        console.log("Requested " + sendedArray.length + " hashes processing");
    }
};

function send(action, data) {

    var postData = {
        "action": action,
        "data": data
    };

    remotePost(listener, postData);
};

function notifyTabs() {

    var notifyArray = processingMd5.filter(function (el) { return el.state == "ready"; });
    if (notifyArray.length > 0) {
        var tabsArray = {};
        for (var i = 0; i < notifyArray.length; i++) {
            var tabId = notifyArray[i].tabId;

            if (!tabsArray[tabId]) {
                tabsArray[tabId] = [];
            }

            tabsArray[tabId].push({ md5: notifyArray[i].md5, result: notifyArray[i].result });
        }

        var props = Object.getOwnPropertyNames(tabsArray);
        for (var x = 0; x < props.length; x++) {
            chrome.tabs.sendMessage(parseInt(props[x]), { md5array: tabsArray[props[x]], idCheck: "me", force: hideStatus });
        }

        removeProcessingHashes(notifyArray);

        console.log("Sended " + notifyArray.length + " results to tabs")
    }
};

function changeProcessingState(array, state)
{
    for (var i = 0; i < processingMd5.length; i++) {
        if (array.some(function (el) { return processingMd5[i].md5 == el.md5; }))
        {
            processingMd5[i].state = state;
        }
    }
};

function removeProcessingHashes(array)
{
    for (var i = processingMd5.length - 1; i >= 0; i--) {
        if (array.some(function (el) { return processingMd5[i].tabId == el.tabId && processingMd5[i].md5 == el.md5; })) {
            processingMd5.splice(i, 1);
        }
    }
}

function tryGetCached(md5)
{
    var result = null;
    for (var i = 0; i < cachedMd5.length; i++) {
        if (cachedMd5[i].md5 == md5) {
            result = cachedMd5[i];
            break;
        }
    }
    return result;
}

function remotePost(url, data, fresponse)
{
    $.ajax({
        type: "POST",
        url: listener,
        data: data,
        dataType: "json",
        timeout: 100, // in milliseconds
        success: fresponse,
        error: function (request, status, err) {
            connection_state = "init";
            console.log("----Connection lost: " + status);
        }
    });
}

chrome.runtime.onMessage.addListener(
    function (request, sender, sendResponse)
    {
        var nativeCounter = 0;
        var cachedCounter = 0;
        if (request.idCheck == "me")
        {
            var md5s = request.md5sToProcess;
            var tId = sender.tab.id;

            var len = md5s.length;
            for (var i = 0; i < len; i++) {

                if (!processingMd5.some(function (el, index, array) { 
                    return el.md5 == md5s[i] && el.tabId == tId;
                }))
                {
                    var cachedValue = tryGetCached(md5s[i]);
                    if (cachedValue != null)
                    {
                        processingMd5.push({
                            tabId: tId,
                            md5: md5s[i],
                            state: "ready",
                            result: cachedValue.result
                        });
                        cachedCounter++;
                    }
                    else
                    {
                        processingMd5.push({
                            tabId: tId,
                            md5: md5s[i],
                            state: "init",
                        });
                        nativeCounter++
                    }                    
                }
            }
        }
        else
        {
            if (request.idCheck == "link")
            {
                chrome.tabs.create({ url: request.link, active: false });
            }

        }

        if (nativeCounter != 0 || cachedCounter != 0) {
            console.log("Request on process from background: " + sender.tab.url
                + "\nPrepared for native request: " + nativeCounter
                + "\nFound in cache: " + cachedCounter);
        }
    });

chrome.browserAction.onClicked.addListener(function (tab) {
    if (hideStatus == "enabled")
    {
        hideStatus = "disabled";
        chrome.browserAction.setIcon({
            path: {
                19: "img/grayscale/icon19g.png",
                38: "img/grayscale/icon38g.png"
            }
        });
    }
    else
    {
        hideStatus = "enabled";
        chrome.browserAction.setIcon({
            path: { 19: "img/normal/icon19.png",
                    38: "img/normal/icon38.png"
                }
        });
    }

    chrome.tabs.query({}, function (tabs) {
        for (var i = 0; i < tabs.length; ++i) {
            chrome.tabs.sendMessage(tabs[i].id, { idCheck: "meSystem", force: hideStatus });
        }
    });
});

chrome.contextMenus.removeAll();
chrome.contextMenus.create({
    title: "Reset cache",
    contexts: ["browser_action"],
    onclick: function () {
        cachedMd5 = [];
    }
});


mainThreadInterval = setInterval(sync, 1000);
sync();