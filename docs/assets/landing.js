(function () {
    const navToggle = document.querySelector("[data-nav-toggle]");
    const nav = document.getElementById("sls-nav");
    const dropdownItems = Array.from(document.querySelectorAll(".sls-nav-has-dropdown"));

    const closeDropdowns = function (except) {
        dropdownItems.forEach(function (item) {
            if (except && item === except) {
                return;
            }

            item.classList.remove("open");
            const trigger = item.querySelector("[data-dropdown-toggle]");
            if (trigger) {
                trigger.setAttribute("aria-expanded", "false");
            }
        });
    };

    const isMobileNav = function () {
        return window.innerWidth <= 992;
    };

    if (navToggle && nav) {
        navToggle.addEventListener("click", function () {
            const isOpen = nav.classList.toggle("open");
            navToggle.setAttribute("aria-expanded", isOpen.toString());

            if (!isOpen) {
                closeDropdowns();
            }
        });

        nav.querySelectorAll("a").forEach(function (link) {
            link.addEventListener("click", function () {
                closeDropdowns();

                if (isMobileNav()) {
                    nav.classList.remove("open");
                    navToggle.setAttribute("aria-expanded", "false");
                }
            });
        });
    }

    dropdownItems.forEach(function (item) {
        const trigger = item.querySelector("[data-dropdown-toggle]");
        if (!trigger) {
            return;
        }

        trigger.addEventListener("click", function (event) {
            event.preventDefault();

            const shouldOpen = !item.classList.contains("open");
            closeDropdowns(shouldOpen ? item : null);
            item.classList.toggle("open", shouldOpen);
            trigger.setAttribute("aria-expanded", shouldOpen.toString());
        });
    });

    document.addEventListener("click", function (event) {
        if (!event.target.closest(".sls-nav")) {
            closeDropdowns();
        }
    });

    window.addEventListener("resize", function () {
        closeDropdowns();

        if (!isMobileNav() && nav) {
            nav.classList.remove("open");
            if (navToggle) {
                navToggle.setAttribute("aria-expanded", "false");
            }
        }
    });

    const revealNodes = document.querySelectorAll(".reveal");
    if (!revealNodes.length || typeof IntersectionObserver === "undefined") {
        revealNodes.forEach(function (node) {
            node.classList.add("is-visible");
        });
        return;
    }

    const observer = new IntersectionObserver(function (entries, obs) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("is-visible");
                obs.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.18,
        rootMargin: "0px 0px -40px 0px"
    });

    revealNodes.forEach(function (node) {
        observer.observe(node);
    });
})();
