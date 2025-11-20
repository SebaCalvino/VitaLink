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


(function () {
    const INTERVALO_REVISION_MS = 60 * 1000;
    const VENTANAS_MINUTOS = [10, 5, 0];


    function ensureToastContainer() {
        let container = document.getElementById("toast-container");
        if (!container) {
            container = document.createElement("div");
            container.id = "toast-container";
            container.style.position = "fixed";
            container.style.top = "20px";
            container.style.right = "20px";
            container.style.zIndex = "9999";
            container.style.display = "flex";
            container.style.flexDirection = "column";
            container.style.gap = "10px";
            container.style.maxWidth = "320px";
            document.body.appendChild(container);
        }
        return container;
    }


    function mostrarToast(mensaje) {
        const container = ensureToastContainer();
        const toast = document.createElement("div");
        toast.className = "vl-toast";
        toast.textContent = mensaje;
        toast.style.background = "#2b3c73";
        toast.style.color = "#fff";
        toast.style.padding = "12px 16px";
        toast.style.borderRadius = "6px";
        toast.style.boxShadow = "0 4px 12px rgba(0,0,0,0.2)";
        toast.style.fontSize = "0.95rem";
        toast.style.opacity = "0";
        toast.style.transition = "opacity 0.3s ease";
        container.appendChild(toast);


        requestAnimationFrame(() => {
            toast.style.opacity = "1";
        });


        setTimeout(() => {
            toast.style.opacity = "0";
            setTimeout(() => toast.remove(), 400);
        }, 5000);
    }


    function parseFrecuenciaHoras(frecuencia) {
        if (!frecuencia) return null;
        const texto = frecuencia.toLowerCase().trim();


        let match = texto.match(/(\d+)\s*(?:hora|horas|hr|hrs)/);
        if (match) {
            return parseInt(match[1], 10);
        }


        match = texto.match(/(\d+)\s*veces\s*(?:al|por)\s*d[ií]a/);
        if (match) {
            const veces = parseInt(match[1], 10);
            return veces > 0 ? 24 / veces : null;
        }


        match = texto.match(/cada\s+(\d+)/);
        if (match) {
            return parseInt(match[1], 10);
        }


        return null;
    }


    function calcularProximaToma(horaProgramada, intervaloMs) {
        if (!horaProgramada || !intervaloMs) return null;


        let proxima = new Date(horaProgramada);
        if (Number.isNaN(proxima.getTime()) || intervaloMs <= 0) return null;


        const ahora = new Date();
        while (proxima < ahora) {
            proxima = new Date(proxima.getTime() + intervaloMs);
        }
        return proxima;
    }


    function prepararMedicamentos(data) {
        if (!Array.isArray(data)) return [];


        return data
            .filter(m => m && m.estado)
            .map(m => {
                const intervaloHoras = parseFrecuenciaHoras(m.frecuencia);
                const intervaloMs = intervaloHoras ? intervaloHoras * 60 * 60 * 1000 : null;
                return {
                    ...m,
                    intervaloMs,
                    proximaToma: calcularProximaToma(m.horaProgramada, intervaloMs),
                    ultimoAviso: null
                };
            })
            .filter(m => m.proximaToma && m.intervaloMs);
    }


    function formatearMensaje(nombre, minutos) {
        if (minutos === 0) {
            return `Tomar ${nombre} ahora`;
        }
        return `Tomar ${nombre} en ${minutos} min`;
    }


    function iniciarNotificacionesMedicamentos() {
        const medicamentos = prepararMedicamentos(window.medicacionesNotificaciones);
        if (!medicamentos.length) return;


        const revisar = () => {
            const ahora = new Date();


            medicamentos.forEach(med => {
                if (!med.proximaToma) return;


                const diffMin = Math.round((med.proximaToma - ahora) / 60000);


                if (VENTANAS_MINUTOS.includes(diffMin) && med.ultimoAviso !== diffMin) {
                    mostrarToast(formatearMensaje(med.nombre, diffMin));
                    med.ultimoAviso = diffMin;
                }


                if (diffMin < 0) {
                    // Programar la siguiente toma
                    let siguiente = new Date(med.proximaToma.getTime() + med.intervaloMs);
                    while (siguiente < ahora) {
                        siguiente = new Date(siguiente.getTime() + med.intervaloMs);
                    }
                    med.proximaToma = siguiente;
                    med.ultimoAviso = null;
                }
            });
        };


        revisar();
        setInterval(revisar, INTERVALO_REVISION_MS);
    }


    document.addEventListener("DOMContentLoaded", iniciarNotificacionesMedicamentos);
})();


(function () {
    const diasContainer = document.querySelector("[data-calendario-dias]");
    const detallePanel = document.querySelector("[data-calendario-detalle]");
    const detalleFecha = document.querySelector(".Detalle-Fecha");
    const detalleTitulo = document.querySelector(".Detalle-Titulo");
    const detalleDireccion = document.querySelector("[data-detalle-direccion]");
    const detalleHora = document.querySelector("[data-detalle-hora]");
    const detalleTipo = document.querySelector("[data-detalle-tipo]");
    const detalleDescripcion = document.querySelector("[data-detalle-descripcion]");
    const detalleCerrar = document.querySelector(".Detalle-Cerrar");
    const mesActual = document.querySelector(".Calendario-MesActual");


    if (!diasContainer || !mesActual) return;


    const estadoCalendario = {
        fecha: new Date(2025, 9, 1),
        eventos: []
    };


    function normalizarEventosIniciales() {
        if (!Array.isArray(window.calendarioEventos)) return;


        estadoCalendario.eventos = window.calendarioEventos
            .map(ev => ({
                ...ev,
                fecha: ev.fecha ? new Date(ev.fecha) : null
            }))
            .filter(ev => ev.fecha instanceof Date && !Number.isNaN(ev.fecha.getTime()));
    }


    function formatearMes(fecha) {
        return fecha.toLocaleDateString("es-AR", { month: "long", year: "numeric" });
    }


    function crearDiaElemento(diaNumero, esActual, tieneEvento, fecha) {
        const btn = document.createElement("button");
        btn.className = "Calendario-Dia";
        btn.type = "button";
        btn.textContent = diaNumero;
        btn.dataset.fecha = fecha.toISOString();


        if (esActual) btn.classList.add("activo");
        if (tieneEvento) {
            btn.classList.add("con-evento");
            const indicador = document.createElement("span");
            indicador.className = "Indicador-Evento";
            btn.appendChild(indicador);
        }


        btn.addEventListener("click", () => mostrarDetalle(fecha));
        return btn;
    }


    function renderizarCalendario() {
        diasContainer.innerHTML = "";
        const fecha = estadoCalendario.fecha;
        const primerDiaSemana = (new Date(fecha.getFullYear(), fecha.getMonth(), 1).getDay() + 6) % 7; // ajuste para lunes
        const diasMes = new Date(fecha.getFullYear(), fecha.getMonth() + 1, 0).getDate();


        mesActual.textContent = formatearMes(fecha).replace(/^\w/, c => c.toUpperCase());


        // Dias vacíos previos
        for (let i = 0; i < primerDiaSemana; i++) {
            const placeholder = document.createElement("div");
            placeholder.className = "Calendario-Dia placeholder";
            diasContainer.appendChild(placeholder);
        }


        const hoy = new Date();
        for (let dia = 1; dia <= diasMes; dia++) {
            const fechaDia = new Date(fecha.getFullYear(), fecha.getMonth(), dia);
            const tieneEvento = estadoCalendario.eventos.some(ev =>
                ev.fecha.getFullYear() === fechaDia.getFullYear() &&
                ev.fecha.getMonth() === fechaDia.getMonth() &&
                ev.fecha.getDate() === fechaDia.getDate());
            const esActual = fechaDia.toDateString() === hoy.toDateString();
            diasContainer.appendChild(crearDiaElemento(dia, esActual, tieneEvento, fechaDia));
        }
    }


    function mostrarDetalle(fecha) {
        const eventosDia = estadoCalendario.eventos.filter(ev =>
            ev.fecha.getFullYear() === fecha.getFullYear() &&
            ev.fecha.getMonth() === fecha.getMonth() &&
            ev.fecha.getDate() === fecha.getDate());


        if (!eventosDia.length) {
            detallePanel.hidden = true;
            return;
        }


        const evento = eventosDia[0];
        detalleFecha.textContent = fecha.toLocaleDateString("es-AR", { day: "numeric", month: "long", year: "numeric" });
        detalleTitulo.textContent = evento.titulo;
        detalleDireccion.textContent = evento.direccion;
        detalleHora.textContent = evento.fecha.toLocaleTimeString("es-AR", { hour: "2-digit", minute: "2-digit" }) + " hs";
        detalleTipo.textContent = evento.tipo === "vacuna" ? "Vacunación" : "Turno médico";
        detalleDescripcion.textContent = evento.descripcion || "Sin descripción";


        detallePanel.hidden = false;
    }


    detalleCerrar?.addEventListener("click", () => {
        detallePanel.hidden = true;
    });


    document.querySelector(".Cal-BtnPrev")?.addEventListener("click", () => {
        estadoCalendario.fecha.setMonth(estadoCalendario.fecha.getMonth() - 1);
        renderizarCalendario();
        detallePanel.hidden = true;
    });


    document.querySelector(".Cal-BtnNext")?.addEventListener("click", () => {
        estadoCalendario.fecha.setMonth(estadoCalendario.fecha.getMonth() + 1);
        renderizarCalendario();
        detallePanel.hidden = true;
    });
    
    


    normalizarEventosIniciales();
    renderizarCalendario();
})();

// Funciones para editar perfil
let campoActual = '';
let valorActual = '';

function abrirModal(campo, valor) {
    campoActual = campo;
    valorActual = valor;
    
    const modal = document.getElementById('modal-editar');
    const campoNombre = document.getElementById('modal-campo-nombre');
    const valorActualSpan = document.getElementById('modal-valor-actual');
    const inputTexto = document.getElementById('modal-nuevo-valor');
    const inputFecha = document.getElementById('modal-nuevo-valor-date');
    
    // Configurar nombres amigables
    const nombresCampos = {
        'Email': 'Email',
        'Nombre': 'Nombre',
        'Apellido': 'Apellido',
        'Doc_nro': 'Número de Documento',
        'FechaNacimiento': 'Fecha de Nacimiento',
        'Sexo': 'Sexo',
        'PesoEnKg': 'Peso',
        'AlturaEnCm': 'Altura',
        'Telefono': 'Teléfono'
    };
    
    campoNombre.textContent = nombresCampos[campo] || campo;
    valorActualSpan.textContent = valor;
    
    // Mostrar el input apropiado según el tipo de campo
    if (campo === 'FechaNacimiento') {
        inputTexto.style.display = 'none';
        inputFecha.style.display = 'block';
        inputFecha.value = valor;
    } else {
        inputTexto.style.display = 'block';
        inputFecha.style.display = 'none';
        inputTexto.value = valor;
    }
    
    modal.style.display = 'flex';
}

function cerrarModal() {
    document.getElementById('modal-editar').style.display = 'none';
    campoActual = '';
    valorActual = '';
}

function guardarCambio() {
    const inputTexto = document.getElementById('modal-nuevo-valor');
    const inputFecha = document.getElementById('modal-nuevo-valor-date');
    const nuevoValor = campoActual === 'FechaNacimiento' ? inputFecha.value : inputTexto.value;
    
    if (!nuevoValor || nuevoValor.trim() === '') {
        alert('Por favor, ingrese un valor válido.');
        return;
    }
    
    // Enviar la petición al servidor
    fetch('/Home/ActualizarCampoUsuario', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            Campo: campoActual,
            Valor: nuevoValor
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Actualizar el valor en la vista
            const elementoValor = document.getElementById('valor-' + campoActual);
            if (elementoValor) {
                if (campoActual === 'FechaNacimiento') {
                    const fecha = new Date(nuevoValor);
                    elementoValor.textContent = fecha.toLocaleDateString('es-ES');
                } else {
                    elementoValor.textContent = nuevoValor;
                }
            }
            cerrarModal();
            alert('Campo actualizado correctamente.');
            // Recargar la página para mostrar los cambios
            location.reload();
        } else {
            alert('Error al actualizar el campo: ' + (data.message || 'Error desconocido'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Ocurrió un error al actualizar el campo.');
    });
}

// Cerrar modal al hacer clic fuera de él
window.addEventListener('click', function(event) {
    const modal = document.getElementById('modal-editar');
    if (event.target === modal) {
        cerrarModal();
    }
});

// Funciones para medicamentos
function toggleIndicaciones(elemento) {
    const card = elemento.closest('.medicamento-card');
    const detalle = card.querySelector('.medicamento-indicaciones-detalle');
    const indicaciones = card.querySelector('.medicamento-indicaciones');
    
    if (detalle.style.display === 'none' || detalle.style.display === '') {
        detalle.style.display = 'block';
        indicaciones.classList.add('active');
    } else {
        detalle.style.display = 'none';
        indicaciones.classList.remove('active');
    }
}

