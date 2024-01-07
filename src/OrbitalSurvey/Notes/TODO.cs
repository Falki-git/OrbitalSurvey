/*
 * 
 * TODO:
 * - GUI buttons for toggling the marker, vessel name and geo coords
 * 
 *
 * TODO NEAR-TERM:
 * - next/previous orbit
 * - revamp of min/alt/max altitudes to be more dynamic
 * - Altimetry Map + legend
 * - scanning parts by -StanWildin
 * 
 * 
 * TODO LONG-TERM: 
 * - see if Slope map can be generated
 * - Discoverables Map (false positives, wide area, high EC consumption, far in the tech tree)
 * - missions triggered by discovering discoverables
 * - scanning triangle in Map scene when active
 * - maybe a settings that continuously keeps the body pixelated/blank/blurry until it's scanned (requested by 123man)
 * - adding waypoints by clicking on the map
 * - zoomable map or a new window with a bigger map
 * - support for custom planet packs (packs will register their maps) 
 *
 * DONE:
 * - save/load persistence of mapping data (POC done)
 * - mark 95% as complete
 * - analytic gathering of data while timewarping - retroactive mapping for higher warp factors
 * - minimum and maximum altitude for mapping
 * - add ideal altitude. Above that viewing cone shrinks. Define max altitude as well (keep in mind body's SOI)
 * - finish scanning module on antennas: add additional info (mix/max altitude, fixed FOV, map percentage, draw overlay?, open map?...)
 * - UITK UI 
 * - handle scene changes (remove overlay, maybe close window?)
 * - change to fixed FOV, keep adjustable for debugging
 * - PAM button for opening GUI
 * - add OAB part descriptions
 * - open exact map from PAM
 * - fix UI dropdowns occupying larger space than they should
 * - change EVE visual texture to the mesh variant
 * - add EC usage
 * - update name: "I Hope You Brought Batteries"
 * - persist window position in save game data
 * - Map scene overlay 
 * - clean up warnings
 * - hide bodies with no data from the dropdown
 * - optimize analytic scanning further
 * - add configurable settings
 * - figure out how to remove PAM entries
 * - Experiments for all map types, 25%, 50%, 75%, 100%
 * - larger FOV for larger commstrength
 * - buffering and transmitting scans (probably redundant with For Science!)
 * - buffer scans until transmitted (probably redundant with For Science!)
 * - transmit to get science
 * - scanning mode progression
 * - larger FOV progression
 * - adjust FOV for large bodies
 * - Region Map
 * - Legend for Region
 * - live position indicators (TODO: vessel registration, unregistration)
 * - status, state, name, lat/lon updates for vessel markers in GUI
 * 
 * Changelog:
 * - fixed bug where unloaded vessels on game load weren't performing active scanning (map wasn't immediately updated)  
 * 
 *
 * 
 */
