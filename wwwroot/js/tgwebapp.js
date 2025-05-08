document.addEventListener('DOMContentLoaded', function () {
    let touchStartX = 0;
    let touchStartY = 0;
    let touchEndX = 0;
    let touchEndY = 0;

    document.addEventListener('touchstart', function (event) {
        touchStartX = event.changedTouches[0].clientX;
        touchStartY = event.changedTouches[0].clientY;
    }, false);

    document.addEventListener('touchend', function (event) {
        touchEndX = event.changedTouches[0].clientX;
        touchEndY = event.changedTouches[0].clientY;

        const deltaX = touchEndX - touchStartX;
        const deltaY = Math.abs(touchEndY - touchStartY);

        const screenHeight = window.innerHeight;
        const screenWidth = window.innerWidth;

        const centerY = screenHeight / 2;
        const touchY = (touchStartY + touchEndY) / 2;
        const isInCenterY = Math.abs(touchY - centerY) < screenHeight / 4;

        const isInLeftZone = touchStartX < screenWidth * 0.4;

        if (deltaX > 100 && deltaY < 75 && isInCenterY && isInLeftZone) {
            window.history.back();
        }
    }, false);
});
