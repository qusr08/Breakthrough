window.customElements.define('bt-build-link', class extends HTMLElement {
    constructor() {
        super();
    }

    connectedCallback() {
        let version = this.getAttribute("version");
        let date = this.getAttribute("date");

        this.innerHTML = `
            <div>${date}: <a href="./v${version}/">v${version}</a></div>
        `;
    }
});