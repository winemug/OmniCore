# OmniCore

OmniCore is a hardware and software platform for controlling and automating drug delivery using Omnipod insulin pumps and Dexcom sensors.

This is the public repository for the OmniCore Mobile App, which can be used to control OmniCore Hardware.

# Tale of two updates (17.Nov.2021)

**Update 1**: [OmniCore Oyster](./README.md#OmniCore-Oyster) is indefinitely delayed, until either the silicon crisis is resolved or we can pay wholesalers large sums to bring it to market as soon as humanly possible. So we will be launching **a crowd-funding campaign** to be able to launch the product as soon as humanly possible. Links will follow.

**Update 2**: Instead of waiting for Oyster, we have decided to **launch [OmniCore RADD](./README.md#OmniCore-RADD) to the general public!** We are adding support for RileyLink back into OmniCore and turn it into a 'companion app' so if you own a legacy setup of [RileyLink](https://getrileylink.org), [xDrip](https://github.com/NightscoutFoundation/xDrip) and **Omnipod Eros** you will be able to see and live **RADD** for yourself. We will also add **Omnipod Dash** support to the OmniCore app in due time, if the Oyster gets further delayed.

See the [FAQ](https://github.com/winemug/OmniCore/wiki/OmniCore-RADD) for more details on what RADD is and does.

# Announcement
<b>We have been working hard behind closed doors on a system to address our current woes with existing products.</b>

Please hold for while we're opening the doors and readying the full announcement of brand new features of the OmniCore ecosystem with more details.

## Supported end devices
Active:
* Omnipod Eros
* Omnipod Dash
* Dexcom G6

Planned:
* Libre 2
* Dexcom G7
* Libre 3

## Features and Components
Monitor and control pods and cgms concurrently.<br/>
Remote access worldwide via cellular IOT networks, local access using bluetooth on your mobile phone.<br/>
OmniCore RADD closed loop system with edge computing. (beta)<br/>

### OmniCore Oyster
A stand-alone hardware device for enabling communication to end devices over the OmniCore API.

* Compact design for bulk minimization.
* Autonomous operation that doesn't require a constant connection to a mobile device or an app.
* Reliable and fast communication with end devices, immune to errors and inconsistencies arising from dependencies.
* Excellent stability and battery life thanks to latest generation of hardware components and the software architecture
* Worldwide network coverage including deep indoors and in wide areas, pre-installed e-sim
* Local connectivitiy over BLE with optional long-range modulation
* Built-in motion sensor and GPS
* Waterproof design, wireless charging, onboard interface for quick actions, over the air updates
* OmniCore RADD ready

### OmniCore Mobile App (this repository)
An Android mobile app for interacting with the OmniCore Oyster locally over BLE or over the internet.
* Manage user profiles and OmniCore Oyster devices
* Register, control and monitor end devices
* Manage and monitor OmniCore RADD

### OmniCore RADD (Reactive Adaptive Drug Delivery)
OmniCore's closed loop algorithm for Type I Diabetes therapy automation. A novel decision making instrument that removes guesswork out of the equation. Currently in a strict opt-in beta.

* Detection of variance in insulin absorption per infusion site.
* Detection of insulin effect based on past and current data.
* Administration of basal insulin, independent of time-of-day.
* Administration of meal-time insulin without the need for user interaction.
* Overlapping administration of insulin and glucagon for shortening periods of hyperglycemia.
* Administration of glucagon to prevent hypoglycemia.

See also: [OmniCore RADD FAQ](https://github.com/winemug/OmniCore/wiki/OmniCore-RADD)


### OmniCore Web
The web interface for providing the same set of functionality as the mobile app.

### OmniCore API
A unified open API for communicating with OmniCore Oyster via BLE and over the internet.

