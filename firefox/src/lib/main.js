var buttons = require('sdk/ui/button/action');
var tabs = require("sdk/tabs");
var data = require("sdk/self").data;
var pageMod = require("sdk/page-mod");
var pageWorkers = require("sdk/page-worker");
var self = require("sdk/self");
var tmr = require('sdk/timers');
var Request = require("sdk/request").Request;
var tabPorts = [];

pageMod.PageMod({
	include: ["*.sankakucomplex.com", 
			"*.iqdb.org", 
			"*.donmai.us", 
			"*.yande.re", 
			"*.konachan.com"],
	contentScriptFile: ["./jquery.min.js", "./content.js"],
	onAttach: function(worker) {
		tabPorts.push(worker.port);
		worker.on('detach', function() {
			var index = tabPorts.indexOf(worker.port);
			if (index !== -1) tabPorts.splice(index, 1);
			clearProcessing();
		});
		worker.port.on('request', function(request) { receiveMessageFromTab(worker.port, request); });
	},
	attachTo: "top",
	contentScriptWhen: "ready"
});

var cachedMd5 = []; // md5, result (True, False, Relative)
var processingMd5 = []; // md5, tabId, result, state = { init, processing, ready }
var hideStatus = "enabled"; // enabled, disabled
var listener = "http://localhost:60025/";
var connection_state = "init"; //init, requested, connected
var mainThreadInterval;
var threadState = "init";

var actionButton = buttons.ActionButton({
  id: "imouto-collection-service",
  label: "Imouto Collection Service",
  icon: { 
			"16": "./normal/icon16.png",
			"18": "./normal/icon18.png",
			"19": "./normal/icon19.png",
			"32": "./normal/icon32.png",
			"36": "./normal/icon36.png",
			"38": "./normal/icon38.png",
			"48": "./normal/icon48.png",
			"64": "./normal/icon64.png",
			"128": "./normal/icon128.png",
			"250": "./normal/icon250.png" 
  },
  onClick: handleClick
});

function handleClick(state) {
    if (hideStatus == "enabled")
    {
        hideStatus = "disabled";
		actionButton.state(actionButton, { "icon": {
			"16": "./grayscale/icon16g.png",
			"18": "./grayscale/icon18g.png",
			"19": "./grayscale/icon19g.png",
			"32": "./grayscale/icon32g.png",
			"36": "./grayscale/icon36g.png",
			"38": "./grayscale/icon38g.png",
			"48": "./grayscale/icon48g.png",
			"64": "./grayscale/icon64g.png",
			"128": "./grayscale/icon128g.png",
			"250": "./grayscale/icon250g.png" 
		}});
    }
    else
    {
        hideStatus = "enabled";
        actionButton.state(actionButton, { "icon": {
			"16": "./normal/icon16.png",
			"18": "./normal/icon18.png",
			"19": "./normal/icon19.png",
			"32": "./normal/icon32.png",
			"36": "./normal/icon36.png",
			"38": "./normal/icon38.png",
			"48": "./normal/icon48.png",
			"64": "./normal/icon64.png",
			"128": "./normal/icon128.png",
			"250": "./normal/icon250.png" 
		}});
    }
	
	// reset cache
	cachedMd5 = [];
	sendMessageToAllTabs({ idCheck: "meSystem", force: hideStatus })
	
	
}

function sendMessageToAllTabs(message) {
  for (var i = 0; i < tabPorts.length; i++) {
        tabPorts[i].emit('response', message);
    }
};

function receiveMessageFromTab(port, request) {
	    var nativeCounter = 0;
        var cachedCounter = 0;
        if (request.idCheck == "me")
        {
            var md5s = request.md5sToProcess;
            var tId = port;

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
                            //state: "ready", //debug
							//result: true	//debug
							state: "init",
                        });
                        nativeCounter++
                    }                    
                }
            }
        }

        if (nativeCounter != 0 || cachedCounter != 0) {
            console.log("Request on process from background"
                + "\nPrepared for native request: " + nativeCounter
                + "\nFound in cache: " + cachedCounter);
        }
};

function tryGetCached(md5) {
    var result = null;
    for (var i = 0; i < cachedMd5.length; i++) {
        if (cachedMd5[i].md5 == md5) {
            result = cachedMd5[i];
            break;
        }
    }
    return result;
}

function clearProcessing() {
	for (var i = 0; i < processingMd5.length; i++)
	{
		var index = tabPorts.indexOf(processingMd5[i].tabId);
		if (index == -1)
		{
			processingMd5.splice(i, 1);
		}
	}
};

function sync() {
    if (connection_state == "init")
    {
        if (threadState == "connected") {
            threadState = "init";
            tmr.clearInterval(mainThreadInterval);
            mainThreadInterval = tmr.setInterval(sync, 1000);
        }
        connect();
    }
    else 
    {
        if (threadState == "init")
        {
            threadState = "connected";
            tmr.clearInterval(mainThreadInterval);
            mainThreadInterval = tmr.setInterval(sync, 100);
        }

        //requestedReadyResponses
        receiveReadyRequest();

        //send needed
        sendToRequestArray();
    }

    notifyTabs();
};

function notifyTabs() {
	clearProcessing();
    var notifyArray = processingMd5.filter(function (el) { return el.state == "ready"; });
    if (notifyArray.length > 0) {
        var tabsArray = {};
        for (var i = 0; i < notifyArray.length; i++) {
            var tabIdIndex = tabPorts.indexOf(notifyArray[i].tabId);
			
            if (!tabsArray[tabIdIndex] && tabIdIndex != -1) {
                tabsArray[tabIdIndex] = [];
            }

            tabsArray[tabIdIndex].push({ md5: notifyArray[i].md5, result: notifyArray[i].result });
        }

        var props = Object.getOwnPropertyNames(tabsArray);
        for (var x = 0; x < props.length; x++) {
            tabPorts[parseInt(props[x])].emit('response', { md5array: tabsArray[props[x]], idCheck: "me", force: hideStatus });
        }

        removeProcessingHashes(notifyArray);

        console.log("Sended " + notifyArray.length + " results to tabs")
    }
};

function removeProcessingHashes(array) {
    for (var i = processingMd5.length - 1; i >= 0; i--) {
        if (array.some(function (el) { return processingMd5[i].tabId == el.tabId && processingMd5[i].md5 == el.md5; })) {
            processingMd5.splice(i, 1);
        }
    }
};

function connect() {
    var postData = {
        "action": "connectRequest"
    };

    console.log("----Connect request");
    remotePost(listener, postData, function (response) {
        //var result = JSON.parse(response);
        var result = response;
        if (result == "\"connectAccept\"")
        {
            connection_state = "connected";
        }
        console.log("----Connected");
    });
}

function remotePost(url, data, fresponse) {
	const {XMLHttpRequest} = require("sdk/net/xhr");
	
	var xhr = new XMLHttpRequest();
	xhr.timeout = 1000;
	xhr.ontimeout = function() {            
		connection_state = "init";
        console.log("----Connection lost: " + this.readyState + " " + this.status);
	}
	
	xhr.onerror = function() {            
		connection_state = "init";
        console.log("----Connection lost: " + this.readyState + " " + this.status);
	}
	
	xhr.onload = function() {
		fresponse(this.responseText); 
	};

	xhr.open('POST', url, true);
	xhr.send(serialize(data));
	
	// var Request = require("sdk/request").Request;
	
	// var latestRequest = Request({
		// url: url,
		// onComplete:  function(response) {
			// if (response.status != 200) {
				// connection_state = "init";
				// console.log("----Connection lost: " + status);
			// }
			// else {
				// fresponse(response.json);
			// }
		// },
		// content: data
	// }).post();

	
    // $.ajax({
        // type: "POST",
        // url: listener,
        // data: data,
        // dataType: "json",
        // timeout: 100, // in milliseconds
        // success: fresponse,
        // error: function (request, status, err) {
            // connection_state = "init";
            // console.log("----Connection lost: " + status);
        // }
    // });
}

serialize = function(obj) {
  var str = [];
  for(var p in obj)
    if (obj.hasOwnProperty(p)) {
      str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
    }
  return str.join("&");
}

function send(action, data) {

    var postData = {
        "action": action,
        "data": data
    };

    remotePost(listener, postData);
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

function changeProcessingState(array, state) {
    for (var i = 0; i < processingMd5.length; i++) {
        if (array.some(function (el) { return processingMd5[i].md5 == el.md5; }))
        {
            processingMd5[i].state = state;
        }
    }
};

function receiveReadyRequest() {

    if (!processingMd5.some(function (element) { return element.state == "processing"; }))
    {
        return;
    }

    var postData = {
        "action": "receiveReady"
    };

    remotePost(listener, postData, function (response) {
        var result = JSON.parse(response);
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
    });
};

mainThreadInterval = tmr.setInterval(sync, 1000);
sync();