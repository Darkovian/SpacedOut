Weather Maker, (c) 2016 Digital Ruby, LLC
http://www.digitalruby.com
Created by Jeff Johnson

Version 1.4

Change Log:
1.4		-	Added procedural sky and procedural textured sky options to sky sphere, see details in the sky sphere section.
		-	Added a sun option to sky sphere - high quality, fast or none, see details in the sky sphere section.
1.3		-	Change how time of day works - it is now a property (seconds) on the day night cycle script. Sun rotation is based on time of day instead of time of day being based off sun rotation now.
		-	Added lat / lon, timezone and day/month/year properties to day night cycle for accurate sun positions and seasons. See "WeatherMakerDayNightCycleScript.cs" for more details.
		-	Small tweaks to make asset easier to jump right into.
		-	Fade directional light in/out as it dips below or above the horizon.
1.2		-	Added a dawn / dusk texture and fade degrees to the sky sphere script.
1.1.3	-	Added a volume property to globally change the volume of all precipitation, wind and thunder sounds.
1.1.2	-	Fixes for particle emission : each precipitation has a new emission rate property that is multiplied by intensity.
1.1.1	-	Small bug fix to 2D particle collisions.
			Toggle the configuration panel with Ctrl-`.
1.1		-	Added day / night cycle with time of day, and sun texture.
1.0		-	Initial Release.

Welcome to Weather Maker! I'm Jeff Johnson, and I've spent hundreds of hours on this asset. This is just the first version and I realize it may not be perfect, may have bugs or pieces missing that you would like. Please send me your suggestions, feedback and bug reports to support@digitalruby.com.

Roadmap:
- More documentation and tutorial videos :)
- Forecast script, set the climate and parameters, and it will make weather for you
- Procedural clouds and fog
- Other celestial objects (moon, etc.)
- More performant and better looking weather effects, mist, snow, etc.
- Precipitation accumulation

Weather Maker is a complete weather solution for Unity. I've created precipitation, lightning, clouds, wind and sound effects. I've even added a sky sphere for picture perfect back drops.

Setup Instructions:
- Drag WeatherMakerPrefab (or for a 2D game, WeatherMakerPrefab2D) into your scene at the root of the hierarchy.
- For 3D, set your main camera far plane to 10000+. This is for clouds and lightning.
- For 3D, if you will be using the built in sky sphere, turn off the Unity skybox on your main camera and in lighting settings.
- Set ambient intensity to 0 for the best night effects.
- Create your own custom script, add a "using DigitalRuby.WeatherMaker" and add a property of "public WeatherMakerScript WeatherScript;" and assign the reference to the script in the inspector.
- You can now use your own script to manipulate the Weather Maker script. See WeatherMakerConfigurationScript.cs for an example of how to do this.

*** IMPORTANT ***
Weather Maker contains a built in GUI to get you started. This GUI lets you control every aspect of the asset. For your final release, you'll want to disable the ConfigurationCanvas object in the WeatherMakerPrefab object, and disable the WeatherMakerConfiguration script on the WeatherMakerPrefab object.
During play mode on a device with a keyboard, Ctrl+` will toggle the configuration panel visibility.

WeatherMakerScript Properties:
- Anchor: The object the weather, sky sphere, etc. should follow. Defaults to the main camera.
- Rain: The rain prefab.
- Snow: The snow prefab.
- Hail: The hail prefab.
- Sleet: The sleet prefab.
- Clouds: The clouds prefab.
- Sky Sphere: The sky sphere prefab.
- Wind: The wind prefab.
- Collision Enabled: Should particles collide with the world?
- Wind Enabled: Turn the wind on or off.
- Sun: The sun.
- Lightning Script: Script for the lightning.
- Precipitation Intensity: Control this value from your script (0 - 1) to set the current precipitation intensity.
- CurrentPrecipitation: Set the current precipitation to null, or one of 4 properties: RainScript, SnowScript, HailScript, SleetScript.
- Precipitation Change Duration: How long it takes to transition / cross fade from one precipitation to another.
- Precipitation Change Threshold: Precipitation changes in intensity must be greater than this value to cross fade, otherwise they happen instantly.

Precipitation
--------------------------------------------------
Weather Maker comes with 4 precipitation prefabs: Rain, snow, hail and sleet. Each are very similar in their setup, but contain different material, textures and parameters.

All the precipitation use WeatherMakerFallingParticleScript. Each has the concept of four stages: none, light, medium and heavy.

Properties:
- Loop Source Light, Medium and Heavy: The audio source to play as the prefab changes to different stages. Each sound fades in and out nicely.
- Sound Intensity Threshold: Change at what point the intensity will go to a new stage. Intensity of 0 is always the none stage.
- Intensity: Change the overall intensity of the precipitation.
- Intensity Multiplier: Change the intensity even more - watch out for performance problems if you go above 1.
- Mist Intensity Multiplier: Make the mist less or more intense - again watch out for performance problems if you go above 1.
- Particle System: The down-pour particle system.
- Mist Particle System: The mist particle system.
- Explosion Particle System: Only used in 2D. For 3D, the explosion particle system is a child object of the particle system.
- Mist Threshold: Once the intensity gets higher than this, mist starts to appear.

3D Properties:
- Height: The down-pour particles will start at this height from the camera.
- Forward Offset: Offset the particle system this many world units behind the camera.
- Mist Height: How high the mist will start falling from.
- For collisions, the weather maker prefab itself has a collisions property.

2D Properties:
- Camera: The camera to show the particles in. Used to scale depending on the orthographic size.
- Height Multiplier: Particles will start at y value of the screen height + (world height * value). A value of 0.15 would make the particles start 15% higher than the screen height.
- Width Multiplier: The particle field will be this much wider than the screen width.
- Collision Mask: What should the particles collide with? The weather maker prefab can control this value globally so don't change this.
- Collision Life Time: About how long particle should live when they collide.
- Explosion Emission Life Time Maximum: When particles collide and have a life time higher than this value, they may emit an explosion.
- Mist Collision Velocity Multiplier: The mist speed is multiplied by this value whenever it collides with something.
- Mist Collision Life Time Multiplier: The mist life time is multiplied by this value whenever it collides with something.

WeatherWindZonePrefab
--------------------------------------------------
This allows wind to randomly blow in your scene, which will affect the rain, snow, mist, etc. Disable if you don't want wind.

Properties:
- Camera: The camera the wind is being created near. Defaults to main camera.
- Loop Source Wind: The looping audio source for the wind.
- Wind Sound Volume Modifier: Raise or lower the wind volume.
- Wind Zone: The Unity wind zone.
- Wind Speed Range: X = Minimum Wind Speed, Y = Maximum Wind Speed, Z = Wind Volume Modifier. Volume = Z / WindSpeed.
- Wind Change Interval: Minimum and maximum random range to change wind speed.
- Allow Blow Up: Can wind blow in the Y direction? Leave off generally.

Sun
--------------------------------------------------
The weather maker prefab contains a Sun object for your convenience. Delete your other directional lights. If you must use your own directional light, disable the Sun object and re-assign all the Sun references in the prefab to your own directional light.

TimeOfDay property is available on WeatherMakerScript.

Clouds
--------------------------------------------------
- Located in WeatherMakerPrefab/Clouds (WeatherMakerCloudScript)

Clouds work differently in 3D compared to 2D. In 3D mode, the clouds are created in a dome shape. In 2D mode, a simple particle system is used to create the clouds. 2D clouds shouldn't need much tweaking. There is a particle system inside the cloud prefab that you can tweak if the clouds don't look exactly how you want.

3D clouds are a little more complicated, so here is how they work:

Properties:
- Material: The cloud material. Usually just leave this to default.
- Material Texture: Allows changing the texture of the clouds. I've included several cloud sprite sheets that you can use (WeatherMaker/Prefab/Textures/Clouds)
- Material Mask Texture: 3D mode only. Can make the clouds softer.
- Tint Color: Change the overall color of the clouds.
- Material Rows: Number of rows in the texture.
- Material Columns: Number of columns in the texture.
- Anchor Clouds: Should the clouds follow the camera? Usually you'll want this on.
- Anchor Offset: Offset the cloud dome by this amount. The default is usually good enough.
- Number of Clouds: Set this to lower values for a more sparse field of clouds.
- Fade Time Range: Range of the number of seconds each cloud will take to fade in or out. Set to 0 for an instant storm.
- Fade Time Delay Range: A cumulative value for each cloud, use this to stagger the creation of clouds. Set to 0,0 for an instant storm. There is no CPU overhead, as all the calculations are done on the GPU.
- Life Time Range: Range of how long the clouds will live, in seconds, before disappearing.
- Radius Range: Range, in world units, of the radius of each cloud.
- Clouds support velocity and angular velocity. For a long storm, you'll want to leave the velocity set to 0. To make the clouds slowly disperse, set the velocity to some value.
- Bounds Radius: The radius of the entire cloud system. Clouds are always created within this bounds.
- Bounds Center: The center of the cloud area. Leave as 0,0,0 unless you have a special use case.
- Bounds Height Range: Clouds created will have a y value within this range.
- Billboard Offset: Clouds created billboard horizontally and tilt vertically as they are further away from the camera. The angle of the camera to the cloud is used to calculate the billboard. Change this value to "pretend" that the camera is this many y units offset from it's current position for purposes of the billboard calculation. Lower values will make the clouds more horizontal.
- Fade Offset: Clouds further away will fade out. Raise this value to decrease the fade effect.

3D Clouds are created in a single mesh for performance.

Lightning
--------------------------------------------------
- Located in WeatherMakerPrefab/ThunderAndLightningPrefab and WeatherMakerPrefab/ThunderAndLightningPrefab/LightningBoltPrefab.
- I've included the core of my Procedural Lightning asset (http://u3d.as/f1c) to power Weather Maker lightning.

ThunderAndLightningPrefab Setup
Ideally this script will work as is. Lightning will be created randomly in a hemisphere around the main camera. Lightning can also be forced visible, which means the script will try to make the bolt appear in the camera field of view.

Lightning light effects are less visible during the day and brighter at night. For best results, ensure that you create clouds before turning the lightning on.

This script has the concept of "Intense" vs. "Normal" lightning. Intense lightning will be created close to the player, be brighter and play a random intense sound very soon after the lightning strike, which will be loud. Normal lightning is further away, plays quieter sounds and those sounds take longer before playing.

WeatherMakerThunderAndLightning Properties:
- Lightning Bolt Script: The lightning bolt script used to generate the lightning. Leave as the default.
- Camera: The camera lightning should be created around. Default is the main camera.
- Lightning Interval Time Range: Random range in seconds between lightning.
- Lightning Intense Probability: Percent change that lightning will be intense (close and loud).
- Thunder Sounds Normal: List of sounds for normal lightning.
- Thunder Sounds Intense: List of sounds for intense lightning.
- Start Y Base: The base y value for lightning to start at.
- Start X Variance: Vary the x start position
- Start Y Variance: Vary the y start position
- Start Z Variance: Vary the z start position
- End X Variance: Vary the x end position
- End Y Variance: Vary the y end position. Lightning will ray-trace to the ground, so the end position may change.
- End Z Variance: Vary the z end position.
- Lightning forced visibility probability: The chance that the lightning bolt will attempt to be visible in the camera.
- Ground lightning chance: The chance that lightning will strike the ground.
- Cloud lightning chance: The chance that lightning will only be visible in the clouds.
- Sun: Used to lessen the lightning brightness during the day.
- Normal Distance: Force lightning away from the camera by this range of distance for normal lightning.
- Intense Distance: Force lightning away from the camera by this range of distance for intense lightning.
- *Note lightning will always be at least normal or intense distance minimum distance from the camera, regardless of Start / End variance settings.

Here are the properties you are most likely to want to change on LightningBoltPrefab (WeatherMakerLightningBoltPrefabScript):
- Duration Range: Range of how long, in seconds, that each lightning bolt will be displayed.
- Trunk width range: Range of possible width, in world units, of the trunk.
- Glow tint color: Change the outer color of the lightning.
- Generations: Change how many line segments the lightning has.
- Glow Intensity: How bright the glow is.
- Glow Width Multiplier: How wide the glow is.
- Lights: To turn off lights, set LightPercent to 0.

Sky Sphere
--------------------------------------------------
- Located in WeatherMakerPrefab/Sky/SkySphere.
- Not used in 2D mode.
- Set texture type to "Advanced". Then disable all settings or set to defaults. Then set wrap mode to clamp, filter mode to bilinear, aniso level 1, set max size to appropriate value and format to automatic truecolor for best appearance.
Modes:
- Sphere: Texture should be about 2:1 aspect ratio. Top half is upper part (sky), bottom half is the ground.
- Panorama: Entire texture is the upper half of the sky sphere only. Great for higher resolution sky when the player won't ever look below the horizon.
- Panorama Mirror Down: Same as Panorama, except that the texture mirrors down to the bottom half of the sky sphere.
- Dome: Texture maps such that the center of the texture is the top of the sky sphere, and the edges of the circle are the horizon. This produces the best looking sky but requires pre-processing of the texture before importing into Unity.
- Dome Mirror Down: Same as dome except that the texture mirros down to the bottom half of the sky sphere.
- Dome Double: Same as dome except the texture contains two domes. The left half is the bottom dome, the right half is the top dome.
- Fish Eye Mirrored: Maps the fish eye to the front of the sky sphere and mirrors it to the back. Not suitable for 360 degree viewing.
- Fish Eye 360: Maps the fish eye to the entire sky sphere. There is a slight distortion at one end of the sky sphere so this is not suitable for full 360 degree viewing.

SkyMode:
- Procedural: Sky is fully procedural and day and dawn dusk sky sphere textures are ignored. Night texture is used as the sun goes below the horizon.
- Procedural Textured: Blends a procedural sky with the textures you specify. Your day and dawn/dusk textures should contain transparent areas with translucent clouds, make sure the import settings for the textures allows transparency. Night texture should still be opaque. The night texture will be hidden by any areas of the dawn/dusk texture that are opaque.
- Textured: The default, sky is fully textured and not procedural.
- Note: Setting the player color space to linear may give a more Earth-like sky color for procedural modes.

SunMode:
- High Quality: Render a high quality sun on to the sky sphere
- Fast: Quickly render a circle sun on the sky sphere
- None: Do not render a sun, you are in charge of making your own sun

Please see WeatherMakerSkySphereScript for full details.

All modes come with example textures (WeatherMaker/Prefab/Textures/SkySphere).

Day / Night Cycle
--------------------------------------------------
WeatherMakerDayNightCycle.cs contains full sun path functionality.

Sun - The directional light representing the sun
SunFullIntensity - The intensity of the sun light at full intensity. Do not modify the sun directional light intensity directly. Set this value instead.
Speed - The speed of the day / night cycle. 0 for no auto progress, 1 for real-time speed, 10 for 10x real-time. Negative numbers cycle backwards.
TimeOfDay - The time of day in local time, in seconds.
Year - the current year, this is currently only used for sun positioning.
Month - the current month, this is currently only used for sun positioning.
Day - the current day of month, this is currently only used for sun positioning.
TimeZoneOffsetSeconds - Used to convert TimeOfDay to UTC time. You should look this up once and set it based on your longitude, year, month and day.
Latitude - Latitude on the planet in degrees (0 - 90)
Longitude - Longitude on the planet in decimal degrees (-180 to 180)
AxisTilt - The tilt of the planet in degrees (earth is about 23.439 degrees)
SunDotFadeThreshold - Begin fading in/out the sun when the dot product of it's direction and the down vector becomes less than or equal to this value.
SunDotDisableThreshold - Disable the sun when the dot product of it's direction and the down vector becomes less than or equal to this value. Useful to prevent artifacts when a directional light shines up through your scene.

Orthographic Size
--------------------------------------------------
For 2D Weather Maker, clouds and lightning might need a little tweaking, depending on your orthographic size. Please be sure to set the cloud anchor offset along with the lightning trunk width and light range to appropriate values for your orthographic size.

Performance Considerations
--------------------------------------------------
Here is a list of things to try if performance is not adequate on your platform:

- Lower the emission value of the particle systems, especially snow, which is the most CPU intensive.
- Turn off mist by rasing the mist threshold to 1.
- Turn off per pixel lighting of mist by setting WeatherMakerFallingParticleScript.EnablePerPixelLightingForMist to false.
- Turn off collisions for particles (CollisionEnabled) property of the WeatherMakerPrefab object.
- Reduce lightning generations and turn off lights (WeatherMakerPrafab/ThunderAndLightningPrefab/LightningBoltPrefab).
- Turn off lightning glow (set glow intensity to 0).
- Turn off soft particles in quality settings.

--------------------------------------------------
Known Issues:
- Sky sphere may have some slight distortion at the poles when using sphere or panorama mode. This is easily fixed by using dome or double dome mode, or by correcting the texture.
- Fish Eye 360 has some distortion at the side pole of the sphere. I have as of yet been unable to fix this. I welcome suggestions and feedback on how to fix this.
- Snow particle system with collision can be very intensive CPU wise. Be careful with the number of particles or turn off collisions.
- Sun can shine through the bottom as it dips below the horizon. Fix this by adding a large volume as the base of your world.

I'm Jeff Johnson, I created Weather Maker just for you. Please email support@digitalruby.com with any feedback, bug reports or suggestions.

- Jeff

Credits:
http://soundbible.com/1718-Hailstorm.html
http://blenderartists.org/forum/archive/index.php/t-24038.html
https://www.binpress.com/tutorial/creating-an-octahedron-sphere/162
http://freesound.org/