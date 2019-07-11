$(document).ready(function () {

    $(window).bind('hashchange', function () {
        applyRoute();

    });

    function applyRoute() {
        var hash = window.location.hash.replace(/^#/, '');
        hash = hash.split("=");
        if (hash[0] == "q") {
            $("#searhResultSection").show();

            $("#spanSearchText").html(decodeURIComponent(hash[1]));

            $('html, body').animate({
                scrollTop: $("#searhResultSection").offset().top
            }, 1500);


            $('#productSeachResult').html("");

            $('#productSeachResult').load("./index_product_search.z?q=" + hash[1]);
        }
        if (hash[0] == "pd" && hash[1] != "hide") {
            $('#productQuickLookModal').attr("product_key", hash[1]);
            $('#productQuickLookModal').modal('show');
        }
        else if (hash[0] == "pd" && hash[1] == "hide") {
            $('#productQuickLookModal').modal('hide');
        }
        else if (hash[0] == "pv" && hash[1] != "hide") {
            $('#productDetailView').attr("product_key", hash[1]);
            $('#productDetailView').modal('show');
        }
        else if (hash[0] == "pv" && hash[1] == "hide") {
            $('#productDetailView').modal('hide');
        }
        else {
            $('.modal').modal('hide');
        }
    };

    applyRoute();
})();