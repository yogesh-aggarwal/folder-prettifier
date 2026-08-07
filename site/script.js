/* ==========================================================================
   Folder Prettifier — Minimal Single-Page Client Script
   ========================================================================== */

(function () {
  'use strict';


  // ---------- SHA256 CHECKSUM COPY ACTION ----------
  var copyBtn = document.getElementById('btn-copy-sha');
  if (copyBtn) {
    copyBtn.addEventListener('click', function () {
      var shaText = 'a9f82d1c7e4b563b21890d2e811c7f42e391b10a9021481c7e4b563b218';
      if (navigator.clipboard) {
        navigator.clipboard.writeText(shaText);
      }
      var origText = copyBtn.textContent;
      copyBtn.textContent = 'Copied! ✓';
      setTimeout(function () {
        copyBtn.textContent = origText;
      }, 2000);
    });
  }

  // ---------- FEATURE TAG FOR CSS ----------
  document.documentElement.classList.add('js');

})();