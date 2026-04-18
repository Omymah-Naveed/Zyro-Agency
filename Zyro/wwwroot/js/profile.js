    $(document).ready(function () {
    // Initialize Isotope
    var $portfolio = $('.portfolio-container').isotope({
        itemSelector: '.portfolio-item',
    layoutMode: 'fitRows'
    });

    // Filter items on button click
    $('#portfolio-flters li').on('click', function () {
        $('#portfolio-flters li').removeClass('filter-active');
    $(this).addClass('filter-active');

    var filterValue = $(this).attr('data-filter');
    $portfolio.isotope({filter: filterValue });
    });
});
