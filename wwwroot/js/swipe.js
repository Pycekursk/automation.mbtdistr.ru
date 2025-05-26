document.addEventListener('DOMContentLoaded', () => {

    let touchStartX = 0;
    let touchStartY = 0;
    let touchEndX = 0;
    let touchEndY = 0;
    let touchStartTime = 0;

    document.addEventListener('touchstart', (e) => {
        if (['INPUT', 'TEXTAREA'].includes(e.target.tagName)) return;

        const touch = e.touches[0];
        touchStartX = touch.clientX;
        touchStartY = touch.clientY;
        touchStartTime = Date.now();
    }, { passive: true });

    document.addEventListener('touchend', (e) => {
        if (['INPUT', 'TEXTAREA'].includes(e.target.tagName)) return;

        const touch = e.changedTouches[0];
        touchEndX = touch.clientX;
        touchEndY = touch.clientY;
        const deltaX = touchEndX - touchStartX;
        const deltaY = Math.abs(touchEndY - touchStartY);
        const elapsedTime = Date.now() - touchStartTime;

        const isSwipeRight = (
            touchStartX < 50 &&                    // свайп начался у левого края
            deltaX > 60 &&                         // достаточная длина по X
            deltaY < 40 &&                         // почти горизонтальный
            elapsedTime < 400                      // короткий по времени
        );

        if (isSwipeRight) {
            console.log('Swipe → detected');
            handleSwipeRight();
        }
    }, { passive: true });

    function handleSwipeRight() {
        // Ваш код (например, возврат назад или закрытие меню)
        history.back();
    }
});