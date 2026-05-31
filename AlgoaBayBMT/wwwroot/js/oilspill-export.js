// Oil spill export helpers: capture a DOM element to PNG and trigger downloads.
window.oilSpillExport = {
    // Capture the element with the given id to a PNG and download it.
    captureElementToPng: async function (elementId, fileName) {
        const element = document.getElementById(elementId);
        if (!element || typeof html2canvas === "undefined") {
            return false;
        }

        try {
            const canvas = await html2canvas(element, {
                backgroundColor: "#ffffff",
                useCORS: true,
                logging: false,
                scale: window.devicePixelRatio || 1
            });

            const dataUrl = canvas.toDataURL("image/png");
            const link = document.createElement("a");
            link.href = dataUrl;
            link.download = fileName || "oilspill-map.png";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            return true;
        } catch (e) {
            console.error("oilSpillExport.captureElementToPng failed", e);
            return false;
        }
    },

    // Capture an element and return the PNG as a base64 string (no download).
    captureElementToBase64: async function (elementId) {
        const element = document.getElementById(elementId);
        if (!element || typeof html2canvas === "undefined") {
            return null;
        }

        try {
            const canvas = await html2canvas(element, {
                backgroundColor: "#ffffff",
                useCORS: true,
                logging: false,
                scale: window.devicePixelRatio || 1
            });
            const dataUrl = canvas.toDataURL("image/png");
            const comma = dataUrl.indexOf(",");
            return comma >= 0 ? dataUrl.substring(comma + 1) : dataUrl;
        } catch (e) {
            console.error("oilSpillExport.captureElementToBase64 failed", e);
            return null;
        }
    },

    // Download a server-generated file provided as a base64 string.
    downloadFile: function (fileName, base64, contentType) {
        try {
            const bytes = atob(base64);
            const buffer = new Uint8Array(bytes.length);
            for (let i = 0; i < bytes.length; i++) {
                buffer[i] = bytes.charCodeAt(i);
            }

            const blob = new Blob([buffer], { type: contentType || "application/octet-stream" });
            const url = URL.createObjectURL(blob);
            const link = document.createElement("a");
            link.href = url;
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
            return true;
        } catch (e) {
            console.error("oilSpillExport.downloadFile failed", e);
            return false;
        }
    }
};
