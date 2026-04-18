    const counters = document.querySelectorAll('.counter');
    const speed = 100; // Lower is faster

    counters.forEach(counter => {
        const updateCount = () => {
            const target = +counter.getAttribute('data-target');
    const count = +counter.innerText.replace('+', '').replace(',', '');
    const increment = Math.ceil(target / speed);

    if (count < target) {
        counter.innerText = count + increment;
    setTimeout(updateCount, 20);
            } else {
        counter.innerText = target.toLocaleString() + '+';
            }
        };

        // Trigger when visible in viewport
        const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                updateCount();
                observer.unobserve(counter); // Run only once
            }
        });
        }, {threshold: 0.5 });

    observer.observe(counter);
    });
