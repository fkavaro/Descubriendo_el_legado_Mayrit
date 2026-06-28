# Descubriendo el legado: Mayrit — El videojuego como ventana al patrimonio invisible del Madrid andalusí

<img width="1290" height="726" alt="Descubriendo el legado Mayrit - Cover (1080p)" src="https://github.com/user-attachments/assets/e837559e-ca9a-4be6-9192-739792cbe063" />


## Sinopsis
**Descubriendo el legado: Mayrit** es un videojuego educativo y de exploración histórica diseñado y desarrollado como Trabajo de Fin de Grado (TFG) para la Universidad Rey Juan Carlos. El proyecto tiene como objetivo unificar, contextualizar y visibilizar el rico patrimonio material e inmaterial del Madrid islámico y medieval (s. IX al s. XIII), a menudo desconocido para la mayoría de la sociedad actual. 

A través de un viaje cronológico dividido en hitos históricos, el jugador puede experimentar el urbanismo y la evolución de la ciudad desde momentos previos a su fundación por el Emir Mohamed I en torno al año 860 d.C. hasta la consolidación de la muralla cristiana en torno al 1230 d.C., redescubriendo el legado andalusí que subyace en la topografía de la capital moderna.

---

## Mecánicas Principales

- **Progresión Cronológica (Milestones):** El juego se divide en 8 hitos históricos independientes que transforman dinámicamente el urbanismo y la sociedad de la escena.
- **Cambio de Perspectiva:** Transición fluida de cámaras gestionada por el usuario. Se puede alternar entre una **cámara aérea de espectador** (para analizar el crecimiento de la ciudad y seleccionar Puntos de Interés) y una **cámara en tercera persona** para controlar directamente al personaje jugable en el terreno.
- **Tours Históricos Guiados (Objetivo Principal):** Cada hito cuenta con un recorrido de paradas fijas (*TourStops*) gestionadas por TourManager y guiadas por una brújula en el HUD.
- **Búsqueda de Coleccionables (Objetivo Secundario):** Para incentivar la orientación espacial y la comprensión del diseño urbano, cada hito esconde objetos coleccionables únicos con pistas sobre su ubicación mostradas en pantalla.
- **Modo de Visualización Moderna:** El HUD integra un componente Switch en el UI Document que permite superponer y activar/desactivar modelos 3D de estructuras modernas (como el Palacio Real o el Teatro Real) sobre el terreno original para comparar el pasado y el presente.
- **Simulación de Vida (NPCs Dinámicos):** Los aldeanos de la ciudad cuentan con rutinas complejas gestionadas por árboles de comportamiento (*Behavior Trees*). Trabajan en edificios y puestos de mercado, acuden a santuarios e interactuan entre sí dinámicamente.

---

## Períodos Históricos (Hitos de Juego)

Las siguientes 8 etapas históricas marcan la evolución de la ciudad:

### 1. Contexto previo a la fundación (Primera mitad del s. IX)
Análisis topográfico y ambiental del terreno original antes del asentamiento islámico.

<img width="2560" height="1440" alt="g0" src="https://github.com/user-attachments/assets/b9af9738-03ea-41af-baa8-c5a4d7cf55c4" />

### 2. Fundación de Mayrit (Segunda mitad del s. IX)
Establecimiento formal de la ciudad por el Emir Mohamed I mediante la construcción de un enclave fortificado (*hisn*).

<img width="2560" height="1440" alt="h1" src="https://github.com/user-attachments/assets/24a8f5de-2820-44a4-a1bf-11426b66d62c" />

### 3. Definición de la almudena (Finales del s. IX)
Construcción de la muralla islámica primigenia en torno al núcleo civil derivado del *hisn*.

<img width="2560" height="1440" alt="b2" src="https://github.com/user-attachments/assets/36c6e235-2053-4fca-8c08-dab252157053" />

### 4. Ataque de Ramiro II (932)
Primer gran conflicto documentado con los reinos cristianos.

<img width="2560" height="1440" alt="c3" src="https://github.com/user-attachments/assets/c339c355-081d-4114-9f5a-a6a19f7396f3" />

### 5. Estancia de Almanzor (977)
Consolidación militar y paso del célebre caudillo andalusí por la plaza de Mayrit.

<img width="2560" height="1440" alt="Milestone4" src="https://github.com/user-attachments/assets/8b4cea3f-41a8-463c-a4e3-280cee2e8b0a" />

### 6. Madraza de Maslama al-Mayriti (1004)
Período de esplendor científico y cultural marcado por la figura del sabio madrileño.

<img width="2560" height="1440" alt="i5" src="https://github.com/user-attachments/assets/b2f90e77-4fe8-4284-857a-535ce42c386b" />

### 7. Conquista y Capitulación (1085)
Transición del poder islámico al cristiano bajo el reinado de Alfonso VI.

<img width="2560" height="1440" alt="j6" src="https://github.com/user-attachments/assets/9dfed22e-bdd4-405b-b539-7cfc491bcf3f" />

### 8. Finalización de la muralla cristiana (1230)
Consolidación de la Villa medieval y cierre de la cerca defensiva castellana.

<img width="2560" height="1440" alt="f7" src="https://github.com/user-attachments/assets/83095e0a-08e0-49df-805a-b2a4e55be7a2" />

---

## Requisitos Técnicos y Optimización

### Stack Tecnológico
- **Motor Gráfico:** Unity (HDRP en su versión 6.3)
- **Herramienta de Interfaz:** UI Toolkit (UIDocument) con la fuente de acento árabe ElMessiri
- **Plataforma Objetivo:** PC (Windows)

### Estrategias de Rendimiento
Para garantizar una tasa de frames fluida renderizando entornos abiertos detallados con cientos de elementos, se integraron las siguientes técnicas:
- **Multi-Scene Workflow:** Flujo de carga asíncrona mediante un `ScenesController`. El juego descarga el menú principal (`MainMenu`) y monta en paralelo la escena de *Gameplay* base junto a la escena específica de datos de oclusión del hito correspondiente.
- **Occlusion Culling por Hito:** Al independizar los hitos en escenas, cada una almacena sus propios datos optimizados para mejorar el rendimiento.
- **Culling de Comportamiento:** Desactivación automática del modelo y el renderizado de mallas de NPCs cuando la cámara se distancia, manteniendo únicamente la ejecución de su lógica subycente.
- **LODs e Imposters:** Configuración de niveles de detalle (*LOD Groups*) en los modelos finales de edificios (con versiones simplificadas en la última franja) y uso de *Imposters* dinámicos en los árboles.
- **Ajustes Avanzados de Render:** Uso de resoluciones dinámicas por *Software Training Post-processing* (STP) de Unity, Antialiasing Temporal (TAA) y optimización de cielos mediante gradientes y capas de nubes planas en lugar de volumétricas costosas.

---

## Instalación y Configuración

1. **Clonar el repositorio:**
    git clone https://github.com/fkavaro/Mayrit.git

2. **Abrir el proyecto:**
    - Inicia Unity Hub.
    - Haz clic en "Add project from disk".
    - Selecciona la carpeta raíz del proyecto.
    - Asegúrate de abrir el proyecto utilizando Unity con soporte para HDRP.

3. **Ejecución inicial:**
    - Para que el ciclo de carga funcione correctamente, abre y ejecuta el juego partiendo siempre desde la escena Assets/Scenes/CoreScene.unity.

---

## Créditos

El videojuego ha sido desarrollado de forma colectiva como Proyecto de Fin de Grado por:

- **Desarrollo de Software, UI/UX, Worldbuilding y Level Design:** Álvaro Moreno García
- **Modelado 3D de Entornos, Edificios, Personajes y Animación:** Francisco Rodríguez Martínez
