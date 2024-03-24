/*
 
 TODO:
 - Discoverables Map
      - trigger mission (what will trigger the mission?)
      - both visual and region mapping must be 100%
      - one discoverable at a time
      - false positives
      - wide area not centered at the discoverable
      - high EC consumption
      - far in the tech tree?
      - register discovered parts of the possible area by looking at a certain radius from the vehicle; only if altitude above ground is less than some number
      - 3 difficulty settings:
          - HARD: no helping whatsoever. Player needs to reach the discoverable science location
          - NORMAL (default):
              - upon landing on a false positive location remove the false positive with a notification that the
                discoverable isn't here.
              - upon landing on the correct location, send a notification that the discoverable is near; "search for it"
          - EASY:
              - upon landing (or just entering the zone?) on the correct location create a waypoint on the discoverable
 - Add config to always show Region legend
 - Add config for triggering science experiments
 
 TODO NEAR-TERM:
 - altimetry Map + legend
 - LAT/LON grid toggle - requested by Schneider21
 
 
 TODO LONG-TERM: 
 - next/previous orbit 
 - support for custom planet packs (packs will register their maps)
 - see if Slope map can be generated
 - scanning triangle in Map scene when active
 - revamp map fetching from the game
 - scanning parts
 - register planted flags?
 - hidden map shown as a pixelated map
 
 BUG FIXES OR IMPROVEMENTS NEEDED:
 - see if I can reproduce scanning data disappearing when returning to KSC
 - experiments get locked in on a particular vessel
 
 DONE:
 - save/load persistence of mapping data (POC done)
 - mark 95% as complete
 - analytic gathering of data while timewarping - retroactive mapping for higher warp factors
 - minimum and maximum altitude for mapping
 - add ideal altitude. Above that viewing cone shrinks. Define max altitude as well (keep in mind body's SOI)
 - finish scanning module on antennas: add additional info (mix/max altitude, fixed FOV, map percentage, draw overlay?, open map?...)
 - UITK UI 
 - handle scene changes (remove overlay, maybe close window?)
 - change to fixed FOV, keep adjustable for debugging
 - PAM button for opening GUI
 - add OAB part descriptions
 - open exact map from PAM
 - fix UI dropdowns occupying larger space than they should
 - change EVE visual texture to the mesh variant
 - add EC usage
 - update name: "I Hope You Brought Batteries"
 - persist window position in save game data
 - Map scene overlay 
 - clean up warnings
 - hide bodies with no data from the dropdown
 - optimize analytic scanning further
 - add configurable settings
 - figure out how to remove PAM entries
 - Experiments for all map types, 25%, 50%, 75%, 100%
 - larger FOV for larger commstrength
 - buffering and transmitting scans (probably redundant with For Science!)
 - buffer scans until transmitted (probably redundant with For Science!)
 - transmit to get science
 - scanning mode progression
 - larger FOV progression
 - adjust FOV for large bodies
 - Region Map
 - Legend for Region
 - live position indicators, vessel registration, unregistration
 - status, state, name, lat/lon updates for vessel markers in GUI
 - GUI buttons for toggling the position marker, vessel name and geo coords
 - notification area at the bottom-right corner
 - close button different styling
 - dropdown controls different styling
 - GEO on mouse over
 - resizable canvas
 - zoomable canvas
 - track active vessel
 - event unregistration
 - change Eve's Olympus color
 - multiple module handling in UI updates
 - load 2k texture on 100%
 - update UITK fonts because of Chinese characters
 - mouse-overing vessel markers now shows vessel names and geo coordinates
 - added Italian translations
 - fixed Chinese fonts not displaying properly in some cases
 - updated Eve's Olympus region color to black so it's easier to differentiate from other regions
 - fixed vessel marking coloring for vessels that have more than one antenna with the same scanning mode
 - replaced region names with the localized version (e.g. Biggest Crater -> Rayed Crater)
 - revamp of min/ideal/max altitudes to be more dynamic
 - adding waypoints by clicking on the map, edit, remove
 - coordinates and region names on mouseover
 - automatically select Body dropdown and MapType dropdown on opening the map from taskbar - requested by rogerawong
 - maybe a setting that continuously keeps the body pixelated/blank/blurry until it's scanned (requested by 123man)
 - Fix: experiment interrupted (not enough resources) log spam during high - reported by Argon
 
 Changelog:
 n/a
 
 
 */