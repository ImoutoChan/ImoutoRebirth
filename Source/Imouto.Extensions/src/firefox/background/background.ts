// toggle showOwnedMediaBorders
chrome.storage.local.set({ showOwnedMediaBorders: true });

chrome.action.onClicked.addListener(async () => {
    const { showOwnedMediaBorders } = await chrome.storage.local.get("showOwnedMediaBorders");
    
    if (showOwnedMediaBorders) {
        console.log('showOwnedMediaBorders: false');
        chrome.storage.local.set({ showOwnedMediaBorders: false });
        
        chrome.action.setIcon({
            path: {
                19: "assets/img/grayscale/icon19g.png",
                38: "assets/img/grayscale/icon38g.png"
            }
        });
    } else {
        console.log('showOwnedMediaBorders: true');
        chrome.storage.local.set({ showOwnedMediaBorders: true });

        chrome.action.setIcon({
            path: {
                19: "assets/img/normal/icon19.png",
                38: "assets/img/normal/icon38.png"
            }
        });
    }
});