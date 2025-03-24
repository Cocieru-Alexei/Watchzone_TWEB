// Simple JavaScript for WatchZone website

// Wait for the document to fully load
document.addEventListener('DOMContentLoaded', function () {

    // Add to cart buttons functionality
    const addToCartButtons = document.querySelectorAll('.btn-primary[data-action="add-to-cart"]');

    if (addToCartButtons.length > 0) {
        addToCartButtons.forEach(function (button) {
            button.addEventListener('click', function () {
                // Get the product name from the closest card or container
                let productName = '';
                let productPrice = '';

                if (this.closest('.card')) {
                    productName = this.closest('.card').querySelector('.card-title').textContent;
                    productPrice = this.closest('.card').querySelector('.price') ?
                        this.closest('.card').querySelector('.price').textContent : '';
                } else if (document.querySelector('h2')) {
                    productName = document.querySelector('h2').textContent;
                }

                alert(`${productName} added to cart!`);
            });
        });
    }

    // Form submission
    const contactForm = document.querySelector('#contactForm');

    if (contactForm) {
        contactForm.addEventListener('submit', function (event) {
            event.preventDefault();
            alert('Thank you for your message! We will contact you soon.');
            contactForm.reset();
        });
    }

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                window.scrollTo({
                    top: target.offsetTop,
                    behavior: 'smooth'
                });
            }
        });
    });
}); 