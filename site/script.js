/* Folder Prettifier - scroll reveal + feature detection.
   Reduced motion is honoured: reveals and stage animation collapse to static. */
(function () {
  var root = document.documentElement;

  // Feature tag is set here (pre-paint inline also does this) for safety.
  root.classList.add("js");

  var reduce = window.matchMedia
    ? window.matchMedia("(prefers-reduced-motion: reduce)").matches
    : false;
  if (reduce) root.classList.add("rm");

  var els = document.querySelectorAll(
    ".tile, .fact, .step, .banner__inner, .section-head, .how__lead, .faq__head, .accordion"
  );

  if (!("IntersectionObserver" in window)) return;

  var io = new IntersectionObserver(
    function (entries) {
      for (var i = 0; i < entries.length; i++) {
        if (entries[i].isIntersecting) {
          entries[i].target.classList.add("is-in");
          io.unobserve(entries[i].target);
        }
      }
    },
    { threshold: 0.14 }
  );

  for (var j = 0; j < els.length; j++) {
    els[j].classList.add("reveal");
    io.observe(els[j]);
  }
})();