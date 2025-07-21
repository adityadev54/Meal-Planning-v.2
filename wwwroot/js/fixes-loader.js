/* Script loader for fixes - to be included in layout */
document.addEventListener('DOMContentLoaded', function() {
    // Helper to load script dynamically
    function loadScript(src, callback) {
        const script = document.createElement('script');
        script.src = src;
        script.onload = callback || function() {};
        document.head.appendChild(script);
    }

    // Detect which page we're on and load appropriate fixes
    const url = window.location.pathname.toLowerCase();
    
    // Load fixes for saved meals page
    if (url.includes('/meals/getsavedmeals') || url.includes('/meals/saved')) {
        loadScript('/js/saved-meals-fixes.js');
    }
    
    // Load fixes for meal generation page
    if (url.includes('/meals/generate') || url.includes('/mealgeneration')) {
        loadScript('/js/meal-generation-fixes.js');
    }
});
