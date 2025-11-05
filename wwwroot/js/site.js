// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function cambiarContrasena() {
    const actual = document.getElementById("contrasenaActual").value.trim();
    const nueva = document.getElementById("contrasenaNueva").value.trim();
    const mensaje = document.getElementById("mensaje");

    if (!actual || !nueva) {
        mensaje.style.color = "red";
        mensaje.textContent = "Por favor complete ambos campos.";
        return;
    }

    fetch("/Usuario/CambiarContrasena", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            contrasenaActual: actual,
            contrasenaNueva: nueva
        })
    })
    .then(r => r.json())
    .then(data => {
        if (data.bloqueado) {
            mensaje.style.color = "red";
            mensaje.textContent = "Se acabaron los intentos. Inténtelo más tarde.";
        } else if (data.success) {
            mensaje.style.color = "green";
            mensaje.textContent = "Contraseña cambiada correctamente.";
            document.getElementById("formCambio").reset();
        } else {
            mensaje.style.color = "red";
            mensaje.textContent = "Contraseña incorrecta. Intento fallido.";
        }
    })
    .catch(() => {
        mensaje.style.color = "red";
        mensaje.textContent = "Error al conectar con el servidor.";
    });
}
