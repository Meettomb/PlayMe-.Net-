﻿function redirectTosignin() {
    window.location.href = "/Sign_in";
}
function redirectTofirstpage() {
    window.location.href = "/index";
}
function redirectTohomepage() {
    window.location.href = "/Home";
} function redirectTodeshbord() {
    window.location.href = "/deshbord";
}

// header Scroll script

const header = document.getElementById('header');

// Add event listener for scroll event
window.addEventListener('scroll', function () {
    // Get the current scroll position
    const scrollPosition = window.pageYOffset || document.documentElement.scrollTop;

    // Check if the scroll position exceeds the threshold (e.g., 100 pixels)
    if (scrollPosition > 100) {
        // Add a class to the header element to change its color
        header.classList.add('scrolled');
    } else {
        // Remove the class from the header element to restore its original color
        header.classList.remove('scrolled');
    }
});



    // Use JavaScript to hide the message after 3 seconds
// hideMessages.js
document.addEventListener("DOMContentLoaded", function () {
    setTimeout(function () {
        var messageDiv = document.getElementById("tempMessage");
        if (messageDiv) {
            messageDiv.style.display = 'none'; // Hide the message
        }
    }, 4000); // 4000 milliseconds = 4 seconds

    setTimeout(function () {
        var messageDiv = document.getElementById("message");
        if (messageDiv) {
            messageDiv.style.display = 'none'; // Hide the message
        }
    }, 4000); // 4000 milliseconds = 4 seconds
});



function sideNav() {
    document.getElementById("sidenavbar").style.width = "300px";
    document.getElementById("Xnavdiv").style.width = "100vw";
    document.getElementById("Xnavdiv").style.height = "100vw";
}

function closeNavbar() {
    document.getElementById("sidenavbar").style.width = "0";
    document.getElementById("Xnavdiv").style.width = "0";
}





//   for popup
document.addEventListener("DOMContentLoaded", function () {
    var settingsLink = document.getElementById("settingsLink");
    var popupOverlay = document.getElementById("popupOverlay");
    var popupBox = document.getElementById("popupBox");

    settingsLink.addEventListener("click", function (event) {
        event.preventDefault();
        popupOverlay.style.display = "block";
        popupBox.classList.add("active");
        document.body.classList.add("no-scroll");
    });

    popupOverlay.addEventListener("click", function (event) {
        if (event.target === popupOverlay) {
            popupOverlay.style.display = "none";
            popupBox.classList.remove("active");
            document.body.classList.remove("no-scroll");
        }
    });
});


// for shoe password
function show() {
    let x = document.getElementById("password");
    let eyeOpen = document.getElementById("mdi-eye");
    let eyeClosed = document.getElementById("eye-closed");
    if (x.type === "password") {
        x.type = "text";
        eyeOpen.style.display = "none";
        eyeClosed.style.display = "inline";
    }
    else {
        eyeOpen.style.display = "inline";
        eyeClosed.style.display = "none";
        x.type = "password";
    }
}
function showforgotepassword1() {
    let x = document.getElementById("NewPassword");
    let eyeOpen = document.getElementById("mdi-eye1");
    let eyeClosed = document.getElementById("eye-closed1");
    if (x.type === "password") {
        x.type = "text";
        eyeOpen.style.display = "none";
        eyeClosed.style.display = "inline";
    }
    else {
        eyeOpen.style.display = "inline";
        eyeClosed.style.display = "none";
        x.type = "password";
    }
}
function showforgotepassword2() {
    let x = document.getElementById("ConfirmPassword");
    let eyeOpen = document.getElementById("mdi-eye2");
    let eyeClosed = document.getElementById("eye-closed2");
    if (x.type === "password") {
        x.type = "text";
        eyeOpen.style.display = "none";
        eyeClosed.style.display = "inline";
    }
    else {
        eyeOpen.style.display = "inline";
        eyeClosed.style.display = "none";
        x.type = "password";
    }
}



// Sorting Data By Name
let isAscending = true; // Variable to track sorting order

function sortdata() {
    const table = document.getElementById("data");
    const tbody = table.querySelector("tbody");
    const rows = Array.from(tbody.querySelectorAll("tr"));
    const sortIcon = document.getElementById("sort-icon");

    // Sort rows based on the second cell (User Name)
    rows.sort((a, b) => {
        const nameA = a.cells[1].textContent.toLowerCase();
        const nameB = b.cells[1].textContent.toLowerCase();
        return isAscending ? nameA.localeCompare(nameB) : nameB.localeCompare(nameA);
    });

    // Append sorted rows back to the tbody
    rows.forEach(row => tbody.appendChild(row));

    // Toggle the sorting order for the next click
    isAscending = !isAscending;

    // Update the icon based on the sorting order
    if (isAscending) {
        sortIcon.classList.remove("mdi-arrow-down-bold");
        sortIcon.classList.add("mdi-arrow-up-bold");
    } else {
        sortIcon.classList.remove("mdi-arrow-up-bold");
        sortIcon.classList.add("mdi-arrow-down-bold");
    }
}

function SortDataByDate() {
    const table = document.getElementById("data");
    const tbody = table.querySelector("tbody");
    const rows = Array.from(tbody.querySelectorAll("tr"));
    const sortIcon = document.getElementById("sort-icon2");

    // Sort rows based on the fifth cell (Date)
    rows.sort((a, b) => {
        const dateA = new Date(a.cells[4].textContent);
        const dateB = new Date(b.cells[4].textContent);
        return isAscending ? dateB - dateA : dateA - dateB;
    });

    // Append sorted rows back to the tbody
    rows.forEach(row => tbody.appendChild(row));

    // Toggle the sorting order for the next click
    isAscending = !isAscending;


    // Update the icon based on the sorting order
    if (isAscending) {
        sortIcon.classList.remove("mdi-arrow-up-bold");
        sortIcon.classList.add("mdi-arrow-down-bold");
    } else {
        sortIcon.classList.remove("mdi-arrow-down-bold");
        sortIcon.classList.add("mdi-arrow-up-bold");
    }

}












const video = document.querySelector("video");
const fullscreen = document.querySelector(".fullscreen-btn");
const playPause = document.querySelector(".play-pause");
const volume = document.querySelector(".volume");
const currentTime = document.querySelector(".current-time");
const duration = document.querySelector(".duration");
const buffer = document.querySelector(".buffer");
const totalDuration = document.querySelector(".total-duration");
const currentDuration = document.querySelector(".current-duration");
const controls = document.querySelector(".controls");
const videoContainer = document.querySelector(".video-container");
const currentVol = document.querySelector(".current-vol");
const totalVol = document.querySelector(".max-vol");
const mainState = document.querySelector(".main-state");
const muteUnmute = document.querySelector(".mute-unmute");
const forward = document.querySelector(".forward");
const backward = document.querySelector(".backward");
const hoverTime = document.querySelector(".hover-time");
const hoverDuration = document.querySelector(".hover-duration");
const miniPlayer = document.querySelector(".mini-player");
const settingsBtn = document.querySelector(".setting-btn");
const settingMenu = document.querySelector(".setting-menu");
const theaterBtn = document.querySelector(".theater-btn");
const speedButtons = document.querySelectorAll(".setting-menu li");
const backwardSate = document.querySelector(".state-backward");
const forwardSate = document.querySelector(".state-forward");
const loader = document.querySelector(".custom-loader");

let isPlaying = false,
    mouseDownProgress = false,
    mouseDownVol = false,
    isCursorOnControls = false,
    muted = false,
    timeout,
    volumeVal = 1,
    mouseOverDuration = false,
    touchClientX = 0,
    touchPastDurationWidth = 0,
    touchStartTime = 0;

// Set initial volume width if the element exists
if (currentVol) {
    currentVol.style.width = volumeVal * 100 + "%";
} else {
    console.warn("currentVol element not found in the DOM");
}

// Video Event Listeners
video.addEventListener("loadedmetadata", canPlayInit);
video.addEventListener("play", play);
video.addEventListener("pause", pause);
video.addEventListener("progress", handleProgress);
video.addEventListener("waiting", handleWaiting);
video.addEventListener("playing", handlePlaying);

document.addEventListener("keydown", handleShorthand);
fullscreen.addEventListener("click", toggleFullscreen);

playPause.addEventListener("click", (e) => {
    if (!isPlaying) {
        play();
    } else {
        pause();
    }
});

duration.addEventListener("click", navigate);

duration.addEventListener("mousedown", (e) => {
    mouseDownProgress = true;
    navigate(e);
});

totalVol.addEventListener("mousedown", (e) => {
    mouseDownVol = true;
    handleVolume(e);
});

document.addEventListener("mouseup", (e) => {
    mouseDownProgress = false;
    mouseDownVol = false;
});

document.addEventListener("mousemove", handleMousemove);

duration.addEventListener("mouseenter", (e) => {
    mouseOverDuration = true;
});
duration.addEventListener("mouseleave", (e) => {
    mouseOverDuration = false;
    hoverTime.style.width = 0;
    hoverDuration.innerHTML = "";
});

videoContainer.addEventListener("click", toggleMainState);
videoContainer.addEventListener("fullscreenchange", () => {
    videoContainer.classList.toggle("fullscreen", document.fullscreenElement);
});
videoContainer.addEventListener("mouseleave", hideControls);
videoContainer.addEventListener("mousemove", (e) => {
    controls.classList.add("show-controls");
    hideControls();
});
videoContainer.addEventListener("touchstart", (e) => {
    controls.classList.add("show-controls");
    touchClientX = e.changedTouches[0].clientX;
    const currentTimeRect = currentTime.getBoundingClientRect();
    touchPastDurationWidth = currentTimeRect.width;
    touchStartTime = e.timeStamp;
});
videoContainer.addEventListener("touchend", () => {
    hideControls();
    touchClientX = 0;
    touchPastDurationWidth = 0;
    touchStartTime = 0;
});
videoContainer.addEventListener("touchmove", handleTouchNavigate);

controls.addEventListener("mouseenter", (e) => {
    controls.classList.add("show-controls");
    isCursorOnControls = true;
});

controls.addEventListener("mouseleave", (e) => {
    isCursorOnControls = false;
});

mainState.addEventListener("click", toggleMainState);

mainState.addEventListener("animationend", handleMainSateAnimationEnd);

muteUnmute.addEventListener("click", toggleMuteUnmute);

muteUnmute.addEventListener("mouseenter", (e) => {
    if (!muted) {
        totalVol.classList.add("show");
    } else {
        totalVol.classList.remove("show");
    }
});

muteUnmute.addEventListener("mouseleave", (e) => {
    if (e.relatedTarget != volume) {
        totalVol.classList.remove("show");
    }
});

forward.addEventListener("click", handleForward);

forwardSate.addEventListener("animationend", () => {
    forwardSate.classList.remove("show-state");
    forwardSate.classList.remove("animate-state");
});

backward.addEventListener("click", handleBackward);

backwardSate.addEventListener("animationend", () => {
    backwardSate.classList.remove("show-state");
    backwardSate.classList.remove("animate-state");
});

miniPlayer.addEventListener("click", toggleMiniPlayer);

theaterBtn.addEventListener("click", toggleTheater);

settingsBtn.addEventListener("click", handleSettingMenu);

speedButtons.forEach((btn) => {
    btn.addEventListener("click", handlePlaybackRate);
});

function canPlayInit() {
    totalDuration.innerHTML = showDuration(video.duration);
    video.volume = volumeVal;
    muted = video.muted;
    if (video.paused) {
        controls.classList.add("show-controls");
        mainState.classList.add("show-state");
        handleMainStateIcon(`<ion-icon name="play-outline"></ion-icon>`);
    }
}

function play() {
    video.play();
    isPlaying = true;
    playPause.innerHTML = `<ion-icon name="pause-outline"></ion-icon>`;
    mainState.classList.remove("show-state");
    handleMainStateIcon(`<ion-icon name="pause-outline"></ion-icon>`);
    // watchProgress();
}

// function watchProgress() {
//   if (isPlaying) {
//     requestAnimationFrame(watchProgress);
//     handleProgressBar();
//   }
// }

video.ontimeupdate = handleProgressBar;

function handleProgressBar() {
    currentTime.style.width = (video.currentTime / video.duration) * 100 + "%";
    currentDuration.innerHTML = showDuration(video.currentTime);
}

function pause() {
    video.pause();
    isPlaying = false;
    playPause.innerHTML = `<ion-icon name="play-outline"></ion-icon>`;
    controls.classList.add("show-controls");
    mainState.classList.add("show-state");
    handleMainStateIcon(`<ion-icon name="play-outline"></ion-icon>`);
    if (video.ended) {
        currentTime.style.width = 100 + "%";
    }
}

function handleWaiting() {
    loader.style.display = "unset";
}

function handlePlaying() {
    loader.style.display = "none";
}

function navigate(e) {
    const totalDurationRect = duration.getBoundingClientRect();
    const width = Math.min(
        Math.max(0, e.clientX - totalDurationRect.x),
        totalDurationRect.width
    );
    currentTime.style.width = (width / totalDurationRect.width) * 100 + "%";
    video.currentTime = (width / totalDurationRect.width) * video.duration;
}

function handleTouchNavigate(e) {
    hideControls();
    if (e.timeStamp - touchStartTime > 500) {
        const durationRect = duration.getBoundingClientRect();
        const clientX = e.changedTouches[0].clientX;
        const value = Math.min(
            Math.max(0, touchPastDurationWidth + (clientX - touchClientX) * 0.2),
            durationRect.width
        );
        currentTime.style.width = value + "px";
        video.currentTime = (value / durationRect.width) * video.duration;
        currentDuration.innerHTML = showDuration(video.currentTime);
    }
}

function showDuration(time) {
    const hours = Math.floor(time / 60 ** 2);
    const min = Math.floor((time / 60) % 60);
    const sec = Math.floor(time % 60);
    if (hours > 0) {
        return `${formatter(hours)}:${formatter(min)}:${formatter(sec)}`;
    } else {
        return `${formatter(min)}:${formatter(sec)}`;
    }
}

function formatter(number) {
    return new Intl.NumberFormat({}, { minimumIntegerDigits: 2 }).format(number);
}

function toggleMuteUnmute() {
    if (!muted) {
        video.volume = 0;
        muted = true;
        muteUnmute.innerHTML = `<ion-icon name="volume-mute-outline"></ion-icon>`;
        handleMainStateIcon(`<ion-icon name="volume-mute-outline"></ion-icon>`);
        totalVol.classList.remove("show");
    } else {
        video.volume = volumeVal;
        muted = false;
        totalVol.classList.add("show");
        handleMainStateIcon(`<ion-icon name="volume-high-outline"></ion-icon>`);
        muteUnmute.innerHTML = `<ion-icon name="volume-high-outline"></ion-icon>`;
    }
}

function hideControls() {
    if (timeout) {
        clearTimeout(timeout);
    }
    timeout = setTimeout(() => {
        if (isPlaying && !isCursorOnControls) {
            controls.classList.remove("show-controls");
            settingMenu.classList.remove("show-setting-menu");
        }
    }, 1000);
}

function toggleMainState(e) {
    e.stopPropagation();
    if (!e.path.includes(controls)) {
        if (!isPlaying) {
            play();
        } else {
            pause();
        }
    }
}

function handleVolume(e) {
    const totalVolRect = totalVol.getBoundingClientRect();
    currentVol.style.width =
        Math.min(Math.max(0, e.clientX - totalVolRect.x), totalVolRect.width) +
        "px";
    volumeVal = Math.min(
        Math.max(0, (e.clientX - totalVolRect.x) / totalVolRect.width),
        1
    );
    video.volume = volumeVal;
}

function handleProgress() {
    if (!video.buffered || !video.buffered.length) {
        return;
    }
    const width = (video.buffered.end(0) / video.duration) * 100 + "%";
    buffer.style.width = width;
}

function toggleFullscreen() {
    if (!document.fullscreenElement) {
        videoContainer.requestFullscreen();
        handleMainStateIcon(`<ion-icon name="scan-outline"></ion-icon>`);
    } else {
        handleMainStateIcon(` <ion-icon name="contract-outline"></ion-icon>`);
        document.exitFullscreen();
    }
}

function handleMousemove(e) {
    if (mouseDownProgress) {
        e.preventDefault();
        navigate(e);
    }
    if (mouseDownVol) {
        handleVolume(e);
    }
    if (mouseOverDuration) {
        const rect = duration.getBoundingClientRect();
        const width = Math.min(Math.max(0, e.clientX - rect.x), rect.width);
        const percent = (width / rect.width) * 100;
        hoverTime.style.width = width + "px";
        hoverDuration.innerHTML = showDuration((video.duration / 100) * percent);
    }
}

function handleForward() {
    forwardSate.classList.add("show-state");
    forwardSate.classList.add("animate-state");
    video.currentTime += 5;
    handleProgressBar();
}

function handleBackward() {
    backwardSate.classList.add("show-state");
    backwardSate.classList.add("animate-state");
    video.currentTime -= 5;
    handleProgressBar();
}

function handleMainStateIcon(icon) {
    mainState.classList.add("animate-state");
    mainState.innerHTML = icon;
}

function handleMainSateAnimationEnd() {
    mainState.classList.remove("animate-state");
    if (!isPlaying) {
        mainState.innerHTML = `<ion-icon name="play-outline"></ion-icon>`;
    }
    if (document.pictureInPictureElement) {
        mainState.innerHTML = ` <ion-icon name="tv-outline"></ion-icon>`;
    }
}

function toggleTheater() {
    videoContainer.classList.toggle("theater");
    if (videoContainer.classList.contains("theater")) {
        handleMainStateIcon(
            `<ion-icon name="tablet-landscape-outline"></ion-icon>`
        );
    } else {
        handleMainStateIcon(`<ion-icon name="tv-outline"></ion-icon>`);
    }
}

function toggleMiniPlayer() {
    if (document.pictureInPictureElement) {
        document.exitPictureInPicture();
        handleMainStateIcon(`<ion-icon name="magnet-outline"></ion-icon>`);
    } else {
        video.requestPictureInPicture();
        handleMainStateIcon(`<ion-icon name="albums-outline"></ion-icon>`);
    }
}

function handleSettingMenu() {
    settingMenu.classList.toggle("show-setting-menu");
}

function handlePlaybackRate(e) {
    video.playbackRate = parseFloat(e.target.dataset.value);
    speedButtons.forEach((btn) => {
        btn.classList.remove("speed-active");
    });
    e.target.classList.add("speed-active");
    settingMenu.classList.remove("show-setting-menu");
}

function handlePlaybackRateKey(type = "") {
    if (type === "increase" && video.playbackRate < 2) {
        video.playbackRate += 0.25;
    } else if (video.playbackRate > 0.25 && type !== "increase") {
        video.playbackRate -= 0.25;
    }
    handleMainStateIcon(
        `<span style="font-size: 1.4rem">${video.playbackRate}x</span>`
    );
    speedButtons.forEach((btn) => {
        btn.classList.remove("speed-active");
        if (btn.dataset.value == video.playbackRate) {
            btn.classList.add("speed-active");
        }
    });
}

function handleShorthand(e) {
    const tagName = document.activeElement.tagName.toLowerCase();
    if (tagName === "input") return;
    if (e.key.match(/[0-9]/gi)) {
        video.currentTime = (video.duration / 100) * (parseInt(e.key) * 10);
        currentTime.style.width = parseInt(e.key) * 10 + "%";
    }
    switch (e.key.toLowerCase()) {
        case " ":
            if (tagName === "button") return;
            if (isPlaying) {
                video.pause();
            } else {
                video.play();
            }
            break;
        case "f":
            toggleFullscreen();
            break;
        case "arrowright":
            handleForward();
            break;
        case "arrowleft":
            handleBackward();
            break;
        case "t":
            toggleTheater();
            break;
        case "i":
            toggleMiniPlayer();
            break;
        case "m":
            toggleMuteUnmute();
            break;
        case "+":
            handlePlaybackRateKey("increase");
            break;
        case "-":
            handlePlaybackRateKey();
            break;

        default:
            break;
    }
}
