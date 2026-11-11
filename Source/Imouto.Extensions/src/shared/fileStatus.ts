export interface KekkaiResponse {
    hash: string;
    status: "NotFound" | "Present" | "RelativePresent";
}

export type FileStatusReply = { results: KekkaiResponse[] } | { error: string };

const token = "";
const endpoint = "http://localhost:11303/FileStatus?token=" + encodeURIComponent(token);

async function getFileStatuses(hashes: string[]): Promise<KekkaiResponse[]> {
    const response = await fetch(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(hashes),
        redirect: "error"
    });
    if (!response.ok) {
        throw new Error("Kekkai HTTP " + response.status);
    }
    return response.json();
}

chrome.runtime.onMessage.addListener((message, _sender, sendResponse: (reply: FileStatusReply) => void) => {
    if (message?.type !== "getFileStatuses") {
        return;
    }
    if (!Array.isArray(message.hashes) || !message.hashes.every(hash => typeof hash === "string" && /^[a-f0-9]{32}$/i.test(hash))) {
        sendResponse({ error: "Expected an array of MD5 hashes" });
        return;
    }

    getFileStatuses(message.hashes).then(
        results => sendResponse({ results }),
        error => sendResponse({ error: String(error) })
    );
    return true; // Keep the message channel open until the POST completes.
});
