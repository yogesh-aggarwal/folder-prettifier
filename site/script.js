/* ==========================================================================
   Folder Prettifier — Client Script
   Fetches latest release metadata from GitHub API (no auth required for
   public repos). Falls back to static values if the request fails.
   ========================================================================== */

(function () {
  'use strict';

  var REPO = 'yogesh-aggarwal/folder-prettifier';
  var API = 'https://api.github.com/repos/' + REPO + '/releases/latest';

  // ---------- HELPERS ----------
  function fmt(bytes) {
    var mb = bytes / (1024 * 1024);
    return mb.toFixed(1) + ' MB';
  }

  function fmtDownloads(n) {
    if (n >= 1000) return (n / 1000).toFixed(1) + 'k downloads';
    return n + ' downloads';
  }

  function el(id) { return document.getElementById(id); }

  // ---------- GITHUB API FETCH ----------
  function loadRelease() {
    fetch(API, { headers: { Accept: 'application/vnd.github+json' } })
      .then(function (r) {
        if (!r.ok) throw new Error('API ' + r.status);
        return r.json();
      })
      .then(function (release) {
        var tag = release.tag_name || '';                       // e.g. "v2.0.0"
        var assets = release.assets || [];

        // Find 64-bit and 32-bit assets by filename pattern
        var asset64 = assets.find(function (a) { return /64/i.test(a.name); });
        var asset32 = assets.find(function (a) { return /32/i.test(a.name); });
        // Fallback: just pick the first asset if no 64/32 naming
        if (!asset64 && assets.length) asset64 = assets[0];
        if (!asset32 && assets.length > 1) asset32 = assets[1];

        // Total downloads across all assets
        var totalDownloads = assets.reduce(function (sum, a) {
          return sum + (a.download_count || 0);
        }, 0);

        // --- Update primary download button ---
        var btn64 = el('btn-download-64');
        if (btn64 && asset64) {
          btn64.href = asset64.browser_download_url;
        } else if (btn64) {
          btn64.href = release.html_url;
        }

        var dlVersion = el('dl-version');
        if (dlVersion) dlVersion.textContent = tag + ' • 64-bit';

        // --- Update meta strip ---
        var dlVersionTag = el('dl-version-tag');
        if (dlVersionTag) {
          dlVersionTag.innerHTML = 'Version <strong>' + tag.replace(/^v/, '') + '</strong>';
        }

        var dlSize = el('dl-size');
        if (dlSize && asset64) dlSize.textContent = fmt(asset64.size);

        var dlCount = el('dl-count');
        if (dlCount && totalDownloads > 0) {
          dlCount.textContent = fmtDownloads(totalDownloads);
        }

        // --- Update 32-bit alt link ---
        var btn32 = el('btn-download-32');
        if (btn32 && asset32) {
          btn32.href = asset32.browser_download_url;
          btn32.textContent = 'Also available: 32-bit (' + fmt(asset32.size) + ')';
        } else if (btn32) {
          btn32.style.display = 'none';
        }
      })
      .catch(function () {
        // Graceful fallback: leave static HTML as-is, just clean placeholder text
        var dlVersion = el('dl-version');
        if (dlVersion) dlVersion.textContent = 'Latest release';

        var dlVersionTag = el('dl-version-tag');
        if (dlVersionTag) dlVersionTag.innerHTML = 'Latest version';

        var dlSize = el('dl-size');
        if (dlSize) dlSize.textContent = 'Standalone .exe';

        var dlCount = el('dl-count');
        if (dlCount) dlCount.textContent = '';

        var btn32 = el('btn-download-32');
        if (btn32) btn32.style.display = 'none';
      });
  }

  // ---------- SHA256 CHECKSUM COPY (legacy, keep for compatibility) ----------
  var copyBtn = el('btn-copy-sha');
  if (copyBtn) {
    copyBtn.addEventListener('click', function () {
      var shaText = copyBtn.previousElementSibling
        ? copyBtn.previousElementSibling.textContent
        : '';
      if (navigator.clipboard && shaText) {
        navigator.clipboard.writeText(shaText);
      }
      var orig = copyBtn.textContent;
      copyBtn.textContent = 'Copied!';
      setTimeout(function () { copyBtn.textContent = orig; }, 2000);
    });
  }

  // ---------- BOOT ----------
  document.documentElement.classList.add('js');
  loadRelease();

})();