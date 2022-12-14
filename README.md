# CitySim

We're simulating a city!

## Unterprojekte

Das Projekt besteht aus den folgenden Unterprojekten:

| Name        | Beschreibung                                                                                                                                                                                                                                                              |
|-------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Backend     | Klassenbibliothek, die das eigentliche Spiels / die eigentliche Simulation ohne Visualisierung bereitstellt.<br/>Durch das Erzeugen einer CitySim Instanz und anschließendem Aufruf der Methode StartAsync kann die Simulation in einem anderem Projekt gestartet werden. |
| Frontend    | Ausführbarer Visualisierungswrapper um das Backend-Projekt.                                                                                                                                                                                                               |
| Benchmark   | Laufzeitanalysen                                                                                                                                                                                                                                                          |                                                                                                                                                                                                                                                         |
| PathFinding | Eine auf 2D-Kacheln basierende Wegfindungsbibliothek von RonenNess: https://github.com/RonenNess/UnityUtils/tree/master/Controls/PathFinding/2dTileBasedPathFinding.                                                                                                      |                                                                                               |

## Team

| Kennung | Name              | Relevante Skills                                                                                                   |
|---------|-------------------|--------------------------------------------------------------------------------------------------------------------|
| @acw954 | Tobias Schulz     | <ul><li>Etwas C#</li><li>WPF</li><li>Neuronale Netze, evolutionär</li></ul>                                        |                                                         
| @acx589 | Julius Held       | <ul><li>C#</li><li>Inkscape</li><li>Visualierung (im Zweifel auch Python)</li><li>Reinforcement Learning</li></ul> |                                                        
| @acx807 | Marlon Regenhardt | <ul><li>C#</li><li>WPF / Blazor</li><li>Python</li><li>CI/CD</li><li>Git</li><li>Neuronale Netze</li></ul>         |                                                       
| @acs521 | Hugo Protsch      | <ul><li>C#</li> <li>WPF</li><li>CI/CD</li></ul>                                                                    |
| @acz494 | Hossam Waziry     | <ul> <li> C# </li><li>Photoshop / Illustrator</li></ul>                                                            |
| @acz361 | Aslam Nabizada    | <ul> <li>Kein c# aber Java </li></ul>                                                                              |

### Verantwortlichkeiten (vorläufig)

- Logik
  - @acs521 Hugo
  - @acz361 Aslam
  - @acx807 Marlon
  - @acw954 Tobias
- Visualisierung Code
    - @acx589 Julius
- Assets
    - (@acx589 Julius)
    - @acz494 Hossam

## Vorgänge

### Bearbeiten von Tickets:
1. Ticket erstellen
   1. Asignee zuordnen
   2. Auf "**in development**" stellen ([Kanban Board](https://git.haw-hamburg.de/mars2022/citysim/-/boards))
2. Neuen Branch erstellen und Ticket bearbeiten
3. MR erstellen
   1. Review anfordern falls nötig
   2. Merge in main
