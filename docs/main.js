window.onload = () => {
    let builds = document.querySelectorAll(".build");

    builds.forEach(element => {
        let date = element.getAttribute('date');
        let version = element.getAttribute('version');
        element.innerHTML += `
            <p><a href="./v${version}/"><strong>v${version}</strong></a> [${date}]</p>
            <p class="tag" style="background-color: var(--color1); color: var(--color2);">dev</p>
            
        `;
    });
}