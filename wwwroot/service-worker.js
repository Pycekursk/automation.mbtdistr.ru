const CACHE_NAME = 'automation-cache-v1';
const urlsToCache = [
  '/botmenu',
  '/manifest.json',
  '/lib/jquery/dist/jquery.js',
  '/lib/bootstrap/dist/css/bootstrap-icons.css',
  '/lib/dx/css/bootstrap.css',
  '/lib/dx/css/dx.dark.compact.css',
  '/js/devextreme/FileSaver.js',
  '/js/devextreme/polyfill.min.js',
  '/js/devextreme/exceljs.js',
  '/js/devextreme/jszip.js',
  '/js/devextreme/dx.all.js',
  '/js/devextreme/aspnet/dx.aspnet.data.js',
  '/js/devextreme/aspnet/dx.aspnet.mvc.js',
  '/js/devextreme/localization/dx.messages.ru.js',
  '/lib/fancybox/fancybox.js',
  '/lib/fancybox/fancybox.css',
  '/js/tgwebapp.js',
  'https://telegram.org/js/telegram-web-app.js'
];

// Установка service worker и кэширование файлов
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME).then(cache => {
      return cache.addAll(urlsToCache);
    })
  );
});

// Работа с запросами
self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request).then(response => {
      return response || fetch(event.request);
    })
  );
});

// Очистка старого кэша при обновлении
self.addEventListener('activate', event => {
  const cacheWhitelist = [CACHE_NAME];
  event.waitUntil(
    caches.keys().then(keyList =>
      Promise.all(
        keyList.map(key => {
          if (!cacheWhitelist.includes(key)) {
            return caches.delete(key);
          }
        })
      )
    )
  );
});
