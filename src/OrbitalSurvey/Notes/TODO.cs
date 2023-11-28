/*
 * TODO:  
 * - minimum and maximum altitude for mapping
 * - finish scanning module on antennas: add additional info (mix/max altitude, fixed FOV, map percentage, draw overlay?, open map?...)
 * - add OAB part descriptions  
 * - buffering and transmitting scans
 * - add EC usage
 * - buffer scans until transmitted
 * - add ideal altitude. Above that viewing cone shrinks. Define max altitude as well (keep in my body's SOI)
 * - UITK UI 
 * 
 * TODO MAYBE:
 * - visual textures are bad/broken... export and bundle them in unity? 
 * 
 * TODO LONG-TERM:
 * - Map scene overlay
 * - scanning rectangle in Map scene when active
 * - altimeter data?
 * - scanning parts
 *
 * TODO SCIENCE:
 * - transmit to get science
 * - scanning mode progression
 * - larger FOV progression
 *
 * DONE:
 * - save/load persistence of mapping data (POC done)
 * - mark 95% as complete
 * - analytic gathering of data while timewarping - retroactive mapping for higher warp factors
 *
 *
 * MISC:
 * 
 * if (GUILayout.Button("QuickSave sound"))
 * {
 *      KSPAudioEventManager.onGameQuickSave();
 *      KSPAudioEventManager.OnResourceTransfertStart();
 * }
 *
 *
 * OAB part description:
 * - scanning cone
 * - minimum altitude
 * - maximum altitude
 * - ideal altitude (do we need this?)
 *
 *
 *
 * ATTRIBUTION:
 * - icon by flaticon.com - <a href="https://www.flaticon.com/free-icons/satellite" title="satellite icons">Satellite icons created by mattbadal - Flaticon</a>
 * 
 * 
 */
