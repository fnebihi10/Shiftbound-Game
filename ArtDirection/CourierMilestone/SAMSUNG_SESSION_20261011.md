Samsung diagnostic session, 11 October 2026

SM-G975U / Adreno 640 / Android 11 / GLES3 / 4 KB pages. Installed QA 0.4.0 code 5 from AndroidHumanCourierCandidate, source 349493c8, APK SHA256 394795204ab27287cde69b00c3b5dd7c41584e0ac9ba3c690348b477f6e6a47b. On-device embedded identity matched the Windows candidate. Previous installed 0.3.1 code 4 APK was pulled before replacement; app data was retained.

Evidence: `.validation/CourierMilestone/Samsung3494`. Rendering evidence reports Mobile, actual 2280×1080, URP scale 1, MSAA 2, HDR/post processing, 1024 shadows, 50 m shadow distance, cap 60. The requested 1920×910 did not become the actual rendering resolution.

Capture ran 20:47:16–21:02:33 UTC, USB charging. Battery 87% / 25.3°C before launch, 83% / 34.3°C at end. Thermal service reported status 0 in collected samples. Raw presentation, battery, thermal and foreground snapshots are retained. Owner played and reported that the game looked good but jumping remained weak, later specifying mainly animation.

SurfaceFlinger app SurfaceView histogram: 54,453 intervals; p50/p95 in the 16 ms bucket, p99 in the 17 ms bucket; 220 intervals in buckets ≥32 ms (0.404%). These are quantized Android histogram bins, not exact frame times. The reported average FPS is unsuitable as the primary result because binning can produce values above the 60 Hz refresh rate.

Sustained representative 60 FPS gameplay: NOT VERIFIED. This session includes idle time, app restarts, screenshots and screen recording; gameplay occupancy is not established for the whole interval. CPU/GPU game bottlenecks and memory/GC allocations remain unmeasured. Do not use the aggregate as a thermal gameplay acceptance pass.

Jump evidence: jump-review.mp4; native two-pointer probe frames/events; gap-calibrated.mp4 and corresponding events. The first gap-video attempt omitted saved control inset/height and veered left: invalid gap-distance evidence. The repeat supplied controlScale 1.052981, inset 35.96165, height 12.41506 and safe-left 107. It travels forward and shows stationary and moving held jumps. Screenshot phase names are intentions: capture overhead delayed later phases beyond the apex. Uninterrupted video is the appropriate temporal evidence. Android screenrecord has no game audio here.

Installation and 4 KB-device startup/rendering: PASS. Physical comfort: NOT VERIFIED beyond owner feedback. Jump animation production quality: FAIL, under active refinement. Actual 16 KB runtime verification: NOT VERIFIED; this phone cannot perform it. Static vendor RELRO compatibility remains FAIL.
