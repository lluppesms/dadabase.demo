
window.validateAndCleanupStorage = function() {
    // Validate theme-mode
    const validThemeModes = ['light', 'dark', 'nineties', 'system'];
    const themeMode = localStorage.getItem('theme-mode');
    if (themeMode && !validThemeModes.includes(themeMode)) {
        console.warn(`Invalid theme-mode value '${themeMode}', clearing.`);
        localStorage.removeItem('theme-mode');
    }
    
    // Validate hit-counter (should be a number)
    const hitCount = localStorage.getItem('hit-counter');
    if (hitCount && isNaN(parseInt(hitCount))) {
        console.warn(`Invalid hit-counter value '${hitCount}', clearing.`);
        localStorage.removeItem('hit-counter');
    }
}

// Run validation on page load
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.validateAndCleanupStorage);
} else {
    window.validateAndCleanupStorage();
}

function focusOnInputField(input) {
    if (input) {
        let element = document.getElementById(input);
        if (element) {
            element.focus();
        }
    }
}
function scrollToBottomOfDiv(input) {
    if (input) {
        let element = document.getElementById(input);
        if (element) {
          element.scrollTop = element.scrollHeight;
        }
    }
}

function syncHeaderTitle() {
    let element = document.getElementById("headerPageTitle");
    if (element) {
        element.innerHTML = document.title;
    }
}
function setHeaderTitle(title) {
    if (title) {
        let element = document.getElementById("headerPageTitle");
        if (element) {
            element.innerHTML = title;
        }
    }
}

// https://docs.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-6.0
async function downloadFileFromStream(fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);

    const url = URL.createObjectURL(blob);

    triggerFileDownload(fileName, url);

    URL.revokeObjectURL(url);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;

    if (fileName) {
        anchorElement.download = fileName;
    }

    anchorElement.click();
    anchorElement.remove();
}

window.clipboardCopy = {
    copyText: function (textToCopy) {
        // navigator clipboard api needs a secure context to work (https)
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(textToCopy);
        } else {
            // use a hidden text area out of viewport to copy the data
            let textArea = document.createElement("textarea");
            textArea.value = textToCopy;
            textArea.style.position = "fixed";
            textArea.style.left = "-999999px";
            textArea.style.top = "-999999px";
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            return new Promise((res, rej) => {
                document.execCommand('copy') ? res() : rej();
                textArea.remove();
            });
        }
    }
}
