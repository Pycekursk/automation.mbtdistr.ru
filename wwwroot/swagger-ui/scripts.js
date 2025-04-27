(function () {
    const key = 'swagger-ui-examples';
    const store = JSON.parse(localStorage.getItem(key) || '{}');
    // при изменении запроса – сохраняем
    const orig = window.ui.getSystem().actions.updateJsonSample;
    window.ui.getSystem().actions.updateJsonSample = args => {
        const id = `${args.method}:${args.path}`;
        store[id] = args.body;
        localStorage.setItem(key, JSON.stringify(store));
        return orig(args);
    };
    // при загрузке – подставляем сохранённые
    window.ui.getSystem().events.on('initializationComplete', () => {
        for (const id in store) {
            const [method, path] = id.split(':');
            window.ui.getSystem().actions.updateJsonSample({ method, path, body: store[id] });
        }
    });
})();
