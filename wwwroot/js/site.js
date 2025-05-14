

$(function () {
    $(window).scroll(function () {
        $('.navbar-collapse').collapse('hide');
    });

    const scrollToTop = document.getElementById("scrollToTop");

    // Показать стрелку при прокрутке
    window.addEventListener("scroll", function () {
        if (window.scrollY > 300) { // Показывать после 300px прокрутки
            scrollToTop.classList.add("visible");
        } else {
            scrollToTop.classList.remove("visible");
        }
    });

    // Плавная прокрутка вверх
    scrollToTop.addEventListener("click", function (e) {
        e.preventDefault();
        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });
    });
})