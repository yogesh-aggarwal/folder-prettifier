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

        // Find the Setup (installer) asset first, then portable x64/x86
        var assetSetup = assets.find(function (a) { return /setup/i.test(a.name); });
        var assetPortable64 = assets.find(function (a) { return /portable.*(x64|64)/i.test(a.name); });
        var assetPortable32 = assets.find(function (a) { return /portable.*(x86|32)/i.test(a.name); });
        // Fallback: just pick the first asset if no setup asset exists
        if (!assetSetup && assets.length) assetSetup = assets[0];
        if (!assetPortable64 && assets.length > 1) assetPortable64 = assets[1];
        if (!assetPortable32 && assets.length > 2) assetPortable32 = assets[2];

        // Total downloads across all assets
        var totalDownloads = assets.reduce(function (sum, a) {
          return sum + (a.download_count || 0);
        }, 0);

        // --- Update primary download button (Setup installer) ---
        var btnSetup = el('btn-download-setup');
        if (btnSetup && assetSetup) {
          btnSetup.href = assetSetup.browser_download_url;
        } else if (btnSetup) {
          btnSetup.href = release.html_url;
        }

        var dlVersion = el('dl-version');
        if (dlVersion) dlVersion.textContent = tag + ' • Setup Installer';

        var dlVersionHero = el('dl-version-hero');
        if (dlVersionHero && tag) dlVersionHero.textContent = tag + ' • Setup Installer';

        // --- Update meta strip ---
        var dlVersionTag = el('dl-version-tag');
        if (dlVersionTag) {
          dlVersionTag.innerHTML = 'Version <strong>' + tag.replace(/^v/, '') + '</strong>';
        }

        var dlSize = el('dl-size');
        if (dlSize && assetSetup) dlSize.textContent = fmt(assetSetup.size);

        var dlCount = el('dl-count');
        if (dlCount && totalDownloads > 0) {
          dlCount.textContent = fmtDownloads(totalDownloads);
        }

        // --- Update portable alt links ---
        var btnPortable64 = el('btn-download-portable-64');
        if (btnPortable64) {
          if (assetPortable64) {
            btnPortable64.href = assetPortable64.browser_download_url;
            btnPortable64.textContent = 'Portable 64-bit (' + fmt(assetPortable64.size) + ')';
          } else {
            btnPortable64.style.display = 'none';
          }
        }

        var btnPortable32 = el('btn-download-portable-32');
        if (btnPortable32) {
          if (assetPortable32) {
            btnPortable32.href = assetPortable32.browser_download_url;
            btnPortable32.textContent = 'Portable 32-bit (' + fmt(assetPortable32.size) + ')';
          } else {
            btnPortable32.style.display = 'none';
          }
        }
      })
      .catch(function () {
        // Graceful fallback: leave static HTML as-is, just clean placeholder text
        var dlVersion = el('dl-version');
        if (dlVersion) dlVersion.textContent = 'Latest release';

        var dlVersionTag = el('dl-version-tag');
        if (dlVersionTag) dlVersionTag.innerHTML = 'Latest version';

        var dlSize = el('dl-size');
        if (dlSize) dlSize.textContent = 'Setup installer';

        var dlCount = el('dl-count');
        if (dlCount) dlCount.textContent = '';

        var btnPortable64 = el('btn-download-portable-64');
        if (btnPortable64) btnPortable64.style.display = 'none';

        var btnPortable32 = el('btn-download-portable-32');
        if (btnPortable32) btnPortable32.style.display = 'none';
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