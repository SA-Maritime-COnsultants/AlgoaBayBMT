(function () {
    // Simple scroll listener to add/remove a class on the logo depending on scroll
    // Use a small throttle to reduce event frequency.
    let ticking = false;
    const el = document.querySelector('.logo');
    function onScroll() {
        if (!el) return;
        if (!ticking) {
            window.requestAnimationFrame(function () {
                const scrolled = window.scrollY || window.pageYOffset;
                // When scrolled more than 40px, reduce the logo translate
                if (scrolled > 40) {
                    el.classList.add('logo-scroll-scrolled');
                } else {
                    el.classList.remove('logo-scroll-scrolled');
                }
                ticking = false;
            });
            ticking = true;
        }
    }
    window.addEventListener('scroll', onScroll, { passive: true });
})();