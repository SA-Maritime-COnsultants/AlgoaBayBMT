using AlgoaBayBMT.Models.Training;

namespace AlgoaBayBMT.Services.Training
{
    internal static class EnvironmentalLawComplianceContent
    {
        public static TrainingModuleCatalog BuildModule()
        {
            var courses = new List<Course>
            {
                BuildCourse1(),
                BuildCourse2(),
                BuildCourse3(),
                BuildCourse4()
            };

            return new TrainingModuleCatalog
            {
                ModuleCode = "M1-ELC",
                Title = "Module 1: Environmental Law & Compliance",
                Description = "Foundational compliance training covering environmental law, DFFE offshore bunkering rules, protected marine areas, and crew environmental responsibilities.",
                ComplianceSummary = "Crew must understand the legal basis for environmental protection, apply offshore bunkering controls, protect sensitive areas, and act immediately when environmental harm is threatened.",
                Courses = courses,
                ModuleCompetencyTasks =
                [
                    BuildModuleCompetencyTask(),
                    BuildEscalationCompetencyTask()
                ],
                ExamPool = courses.SelectMany(x => x.QuestionBank).ToList()
            };
        }

        private static Course BuildCourse1()
        {
            var lessons = new List<Lesson>
            {
                new()
                {
                    LessonCode = "C1-L1",
                    Slug = "why-environmental-law-exists",
                    Title = "Why Environmental Law Exists",
                    NarrativeHtml = "<p>Environmental law exists to prevent harm before it happens, protect shared marine resources, and define accountability when pollution occurs. For offshore bunkering, the law turns good practice into mandatory control measures.</p><p>Crew should understand that environmental law is operational law. It influences planning, reporting, watchkeeping, waste handling, emergency response, and post-incident evidence preservation.</p>",
                    Slides =
                    [
                        Slide("Purpose of Environmental Law", "Environmental law manages risk before damage becomes irreversible.", "Protect ecosystems and livelihoods", "Set enforceable duties", "Create reporting and response obligations"),
                        Slide("Why It Matters Offshore", "Offshore operations can spread impacts rapidly through currents, weather, and vessel movement.", "Small releases can escalate quickly", "Sensitive habitats may be close to operations", "Evidence can disappear if not preserved early"),
                        Slide("Crew-Level Impact", "Every crew member contributes to compliance.", "Follow approved procedures", "Stop unsafe acts", "Report deviations immediately"),
                        Slide("Compliance Mindset", "Environmental compliance is both preventive and traceable.", "Plan the job", "Verify barriers", "Record what happened", "Escalate early")
                    ],
                    KnowledgeChecks =
                    [
                        Mcq("C1KC1", "ELC-C1", "C1-L1", "Which statement best explains the operational value of environmental law offshore?", "It converts environmental protection into practical duties that crews must follow.", 1, "It applies only after a prosecution starts", "It defines mandatory controls and response duties", "It replaces company procedures", "It only concerns regulators on shore"),
                        Tf("C1KC2", "ELC-C1", "C1-L1", "Environmental law is relevant only to masters and company managers.", false, "Environmental compliance applies to all personnel involved in planning, execution, monitoring, and reporting."),
                        Mcq("C1KC3", "ELC-C1", "C1-L1", "Which action best supports legal defensibility after an environmental incident?", "Preserving accurate evidence and reporting timelines supports traceability.", 2, "Changing logs to improve clarity", "Waiting for a manager before recording anything", "Preserving evidence and recording facts promptly", "Only taking photographs after cleanup" )
                    ]
                },
                new()
                {
                    LessonCode = "C1-L2",
                    Slug = "legal-framework-and-duty-of-care",
                    Title = "Legal Framework and Duty of Care",
                    NarrativeHtml = "<p>Environmental compliance sits inside a layered framework: constitutional principles, environmental statutes, permit conditions, company procedures, and site-specific controls. Crew do not need to memorize every legal section, but they must understand the duty of care standard.</p><p>Duty of care means taking reasonable steps to avoid pollution, limit damage, and report issues without delay. If a risk is foreseeable, it must be controlled.</p>",
                    Slides =
                    [
                        Slide("Layered Compliance", "Environmental obligations rarely come from one source.", "National law", "Regulator directives", "Permit conditions", "Company procedures"),
                        Slide("Duty of Care", "Reasonable steps must be taken to prevent, minimise, and remedy pollution.", "See the risk", "Control the risk", "Report the risk", "Document the response"),
                        Slide("Operational Translation", "Legal duties become permits, checklists, hold points, and escalation triggers.", "Pre-transfer checks", "Exclusion zones", "Weather limits", "Emergency readiness"),
                        Slide("If in Doubt", "Where uncertainty exists, crews should adopt the safer and more environmentally protective option.", "Pause", "Clarify", "Escalate", "Do not improvise outside procedure")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C1KC4", "ELC-C1", "C1-L2", "Duty of care means crews must take reasonable steps to prevent or minimise pollution.", true, "Duty of care is a practical prevention obligation, not only a reporting obligation."),
                        Mcq("C1KC5", "ELC-C1", "C1-L2", "What should a crew member do if an operation falls outside the approved environmental controls?", "Escalate and pause until the deviation is assessed and authorised.", 3, "Continue if the schedule is tight", "Use personal judgment and proceed", "Ignore the issue if no spill occurred yet", "Pause and escalate through the proper chain"),
                        Tf("C1KC6", "ELC-C1", "C1-L2", "Permit conditions can be less important than company preference during offshore transfer.", false, "Permit conditions form part of the binding control framework and cannot be overridden informally.")
                    ]
                },
                new()
                {
                    LessonCode = "C1-L3",
                    Slug = "pollution-prevention-and-reporting",
                    Title = "Pollution Prevention and Reporting",
                    NarrativeHtml = "<p>Environmental compliance is strongest when prevention barriers are checked before work starts and reporting triggers are understood before something goes wrong. Good reporting is factual, timely, and complete.</p><p>Crews should know what conditions require immediate pause, what information regulators and company responders need, and how to preserve records that show the sequence of events.</p>",
                    Slides =
                    [
                        Slide("Prevention First", "The easiest spill to manage is the one that never occurs.", "Check equipment integrity", "Verify communications", "Confirm weather and sea state", "Review emergency equipment"),
                        Slide("Trigger Points", "Certain events demand immediate pause and notification.", "Unexpected leak", "Equipment failure", "Loss of containment risk", "Drift outside safe envelope"),
                        Slide("Good Reporting", "Reports should be factual and chronological.", "What happened", "When and where", "What was done", "Who was informed"),
                        Slide("Evidence Preservation", "Preserve logs, positions, photos, witness details, and equipment status.", "Do not overwrite data", "Do not speculate", "Keep originals", "Capture timestamps")
                    ],
                    KnowledgeChecks =
                    [
                        Mcq("C1KC7", "ELC-C1", "C1-L3", "Which report quality is most important immediately after a suspected release?", "Reports should be factual and timely.", 0, "Factual and timely", "Lengthy and technical", "Delayed until all answers are known", "Restricted to senior officers only"),
                        Tf("C1KC8", "ELC-C1", "C1-L3", "Evidence should be preserved even when the scale of impact is still unclear.", true, "Early preservation protects the accuracy of later investigations and reporting."),
                        Mcq("C1KC9", "ELC-C1", "C1-L3", "Which condition should normally trigger an immediate operational pause?", "Any developing loss-of-containment risk should trigger pause and escalation.", 2, "Minor schedule delay", "Routine watch change", "Unexpected leak or containment concern", "A completed checklist" )
                    ]
                },
                new()
                {
                    LessonCode = "C1-L4",
                    Slug = "environmental-law-in-practice",
                    Title = "Environmental Law in Practice",
                    NarrativeHtml = "<p>Legal compliance depends on visible operational behaviours: challenge drift from procedure, maintain barrier discipline, stop work when control conditions fail, and communicate clearly with the control room and vessel teams.</p><p>Environmental law is therefore not abstract. It is visible in the quality of permits, toolbox talks, alarms, handovers, logs, and emergency readiness.</p>",
                    Slides =
                    [
                        Slide("Behavioural Indicators", "Strong compliance is visible in routine work.", "Good handovers", "Accurate logs", "Challenge-response communication", "Stop-work discipline"),
                        Slide("Weak Signals", "Small deviations often precede bigger events.", "Unclear roles", "Rushed checklists", "Poor watchkeeping", "Unreported near misses"),
                        Slide("Leadership at All Levels", "Anyone can escalate a compliance concern.", "Speak up", "Use formal channels", "Confirm receipt", "Follow through"),
                        Slide("Outcome", "Operational excellence and legal compliance support one another.", "Safer transfers", "Cleaner operations", "Better audit trails", "Stronger regulator confidence")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C1KC10", "ELC-C1", "C1-L4", "Rushed checklists and unclear communication can be early signs of compliance failure.", true, "Weak operational discipline often precedes environmental incidents."),
                        Mcq("C1KC11", "ELC-C1", "C1-L4", "What best demonstrates environmental law in practice?", "Using procedures, records, and escalation correctly during real operations.", 1, "Only quoting legislation during audits", "Applying controls and escalation during work", "Delegating compliance to shore staff", "Avoiding written records"),
                        Tf("C1KC12", "ELC-C1", "C1-L4", "Only the environmental officer may stop an operation for environmental reasons.", false, "Any person who identifies an unsafe or non-compliant condition should escalate and stop work according to procedure.")
                    ]
                }
            };

            return new Course
            {
                CourseCode = "ELC-C1",
                Slug = "introduction-to-environmental-law",
                Title = "Course 1: Introduction to Environmental Law",
                Overview = "Builds legal awareness for offshore operations by translating environmental law into practical duties, reporting triggers, and defensible behaviours.",
                RegulatoryReference = "National environmental governance principles, duty-of-care obligations, company environmental controls.",
                Audience = "All crew, masters, officers, POACs, and shore-based coordinators.",
                EstimatedMinutes = 75,
                Lessons = lessons,
                Flashcards =
                [
                    Flashcard("Legal principle", "Duty of care", "Take reasonable steps to prevent, minimise, and remedy pollution."),
                    Flashcard("Operational behaviour", "Stop-work discipline", "Pause the job when environmental controls fail or uncertainty increases."),
                    Flashcard("Reporting", "Good incident reporting", "Factual, timely, chronological, and evidence-based."),
                    Flashcard("Traceability", "Evidence preservation", "Protect logs, images, timestamps, witness statements, and equipment status."),
                    Flashcard("Control concept", "Layered compliance", "Legal duties are expressed through permits, procedures, checklists, and hold points."),
                    Flashcard("Risk principle", "Foreseeable risk", "If a hazard can be anticipated, it must be controlled before work continues.")
                ],
                ScenarioTasks = [BuildCourse1Scenario()],
                QuestionBank = BuildCourse1Questions()
            };
        }

        private static Course BuildCourse2()
        {
            var lessons = new List<Lesson>
            {
                new()
                {
                    LessonCode = "C2-L1",
                    Slug = "regulatory-intent-and-scope",
                    Title = "Regulatory Intent and Scope",
                    NarrativeHtml = "<p>DFFE offshore bunkering controls exist to reduce environmental harm in a high-consequence operating environment. The controls usually define where operations may occur, under what conditions they may proceed, what must be reported, and what records must be kept.</p><p>For crews, this means planning work so every transfer stays inside approved environmental boundaries.</p>",
                    Slides =
                    [
                        Slide("Why DFFE Controls Matter", "Offshore bunkering can affect birds, mammals, fisheries, and coastal livelihoods.", "Prevent avoidable pollution", "Control location and timing", "Set response expectations"),
                        Slide("Scope of Control", "Regulation usually applies to both preparation and execution.", "Vessel readiness", "Site suitability", "Weather criteria", "Reporting and evidence"),
                        Slide("What Crew Must Know", "Do not treat permits as shore-only documents.", "Operating limits", "No-go conditions", "Required notifications", "Documentation expectations"),
                        Slide("Compliance Objective", "Stay within authorised conditions at all times.", "Know the envelope", "Monitor drift", "Escalate deviations")
                    ],
                    KnowledgeChecks =
                    [
                        Mcq("C2KC1", "ELC-C2", "C2-L1", "What is the main intent of DFFE offshore bunkering controls?", "They aim to reduce environmental harm by controlling how, where, and when operations occur.", 2, "To speed up transfer approvals", "To replace vessel procedures", "To reduce environmental harm through controlled operations", "To remove watchkeeping requirements"),
                        Tf("C2KC2", "ELC-C2", "C2-L1", "Permit conditions should be treated as operational controls, not only administrative paperwork.", true, "Permit conditions define the authorised operating envelope."),
                        Mcq("C2KC3", "ELC-C2", "C2-L1", "If the operation moves outside the authorised envelope, what is the correct response?", "Pause, assess, and escalate immediately.", 1, "Continue if transfer has already started", "Pause and escalate", "Wait for the next reporting period", "Only log the issue after completion")
                    ]
                },
                new()
                {
                    LessonCode = "C2-L2",
                    Slug = "pre-transfer-compliance-controls",
                    Title = "Pre-Transfer Compliance Controls",
                    NarrativeHtml = "<p>Before product moves, crews must confirm that pre-transfer checks are complete, communication channels are tested, emergency equipment is available, environmental watch requirements are understood, and weather remains within the authorised window.</p><p>Transfers should not begin because the vessel is ready alone; the environmental conditions and permit conditions must also be ready.</p>",
                    Slides =
                    [
                        Slide("Pre-Transfer Gate", "No product movement before environmental and operational readiness are confirmed.", "Checklist completion", "Communication test", "Weather check", "Emergency equipment readiness"),
                        Slide("Environmental Watch", "Watchkeepers should know what to look for and when to escalate.", "Surface sheen", "Wildlife presence", "Drift changes", "Equipment leaks"),
                        Slide("Dynamic Conditions", "Readiness can change quickly offshore.", "Reconfirm weather", "Reconfirm positions", "Reconfirm safe separation", "Reconfirm support availability"),
                        Slide("Stop Before Start", "Beginning under marginal conditions increases legal and environmental risk.", "Do not rationalise", "Do not compress checks", "Do not skip hold points")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C2KC4", "ELC-C2", "C2-L2", "A completed technical checklist alone is enough to start transfer if environmental conditions have changed.", false, "Environmental conditions must remain within the authorised operating envelope."),
                        Mcq("C2KC5", "ELC-C2", "C2-L2", "Which item is part of a credible pre-transfer environmental readiness check?", "Testing communications and confirming no-go triggers are understood.", 0, "Testing communications and confirming no-go triggers", "Waiting until first product movement to check for sheen", "Reducing lookout frequency to save time", "Skipping wildlife watch assignments"),
                        Tf("C2KC6", "ELC-C2", "C2-L2", "Environmental watch requirements should be briefed before transfer begins.", true, "Crew must know what they are expected to monitor and report before the operation starts.")
                    ]
                },
                new()
                {
                    LessonCode = "C2-L3",
                    Slug = "during-transfer-monitoring-and-escalation",
                    Title = "During-Transfer Monitoring and Escalation",
                    NarrativeHtml = "<p>Once transfer starts, compliance depends on continuous verification. Positions, hoses, couplings, sea state, lighting, traffic, and environmental observations must remain inside the safe operating envelope.</p><p>Escalation should be immediate when trends suggest the envelope is being lost.</p>",
                    Slides =
                    [
                        Slide("Continuous Verification", "Transfer safety depends on constant monitoring, not once-off confirmation.", "Watch the transfer system", "Watch vessel behaviour", "Watch the sea surface", "Watch the environment"),
                        Slide("Escalation Triggers", "Escalate trends before they become incidents.", "Unexpected movement", "Sheen indication", "Alarms or pressure anomalies", "Communication degradation"),
                        Slide("Pause Criteria", "A pause is a compliance control, not a failure.", "Investigate", "Stabilise", "Communicate", "Resume only when safe"),
                        Slide("Operational Discipline", "Control drift often begins with normalisation of small problems.", "Challenge deviations", "Do not accept workarounds", "Keep logs current")
                    ],
                    KnowledgeChecks =
                    [
                        Mcq("C2KC7", "ELC-C2", "C2-L3", "How should crews treat a growing pressure anomaly during transfer?", "As an escalation trigger requiring investigation before risk increases further.", 3, "Ignore it if transfer continues", "Record it only after completion", "Treat it as normal offshore variation", "Escalate and assess immediately"),
                        Tf("C2KC8", "ELC-C2", "C2-L3", "Pausing a transfer to confirm containment is aligned with compliance expectations.", true, "A pause protects the environment and the defensibility of the operation."),
                        Tf("C2KC9", "ELC-C2", "C2-L3", "Minor communication degradation can be ignored if both vessels remain visible.", false, "Reliable communication is a core control, not an optional convenience.")
                    ]
                },
                new()
                {
                    LessonCode = "C2-L4",
                    Slug = "post-transfer-reporting-and-closeout",
                    Title = "Post-Transfer Reporting and Closeout",
                    NarrativeHtml = "<p>Post-transfer compliance includes accurate closeout, logging deviations, confirming that equipment is secure, and ensuring that environmental observations are recorded. If something abnormal occurred, closeout records should show what happened and how the crew responded.</p>",
                    Slides =
                    [
                        Slide("Closeout Discipline", "A compliant transfer ends with evidence, not assumptions.", "Log completion", "Record deviations", "Verify equipment condition", "Capture environmental observations"),
                        Slide("Near-Miss Value", "Near-miss reporting prevents repetition.", "Identify weak barriers", "Improve procedures", "Strengthen training"),
                        Slide("Regulator Confidence", "Clear records show that the operation was controlled and supervised.", "Chronology matters", "Facts matter", "Corrective actions matter"),
                        Slide("Learning Loop", "Closeout feeds future readiness.", "Debrief", "Update controls", "Share lessons")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C2KC10", "ELC-C2", "C2-L4", "Post-transfer closeout should include abnormal observations even if no pollution occurred.", true, "Deviations and near misses are important compliance and learning signals."),
                        Mcq("C2KC11", "ELC-C2", "C2-L4", "Why is near-miss reporting important after bunkering?", "It helps identify weak controls before a future incident occurs.", 1, "It is only useful for insurance claims", "It helps improve controls before a future incident", "It replaces equipment maintenance", "It avoids the need for debriefs"),
                        Tf("C2KC12", "ELC-C2", "C2-L4", "If the transfer finishes on schedule, detailed closeout records are less important.", false, "Schedule success does not remove legal and operational recordkeeping duties.")
                    ]
                }
            };

            return new Course
            {
                CourseCode = "ELC-C2",
                Slug = "dffe-offshore-bunkering-regulations",
                Title = "Course 2: DFFE Offshore Bunkering Regulations",
                Overview = "Explains how DFFE controls translate into real offshore bunkering planning, monitoring, escalation, and closeout behaviour.",
                RegulatoryReference = "Applicable DFFE offshore bunkering controls, permit conditions, environmental monitoring obligations.",
                Audience = "Operational leadership, watchkeepers, POACs, transfer teams, and support personnel.",
                EstimatedMinutes = 90,
                Lessons = lessons,
                Flashcards =
                [
                    Flashcard("Authorisation", "Operating envelope", "The combination of approved location, conditions, weather limits, and control barriers."),
                    Flashcard("Preparation", "Pre-transfer gate", "The point at which all technical, environmental, and communication controls must be confirmed."),
                    Flashcard("Monitoring", "Continuous verification", "Keeping transfer and environmental conditions under active review throughout the operation."),
                    Flashcard("Escalation", "Pause criteria", "Defined conditions that require transfer to stop until the risk is understood and controlled."),
                    Flashcard("Closeout", "Near-miss reporting", "Recording weak signals and deviations so future controls can improve."),
                    Flashcard("Compliance", "Permit discipline", "Treating permit conditions as binding operational requirements.")
                ],
                ScenarioTasks = [BuildCourse2Scenario()],
                QuestionBank = BuildCourse2Questions()
            };
        }

        private static Course BuildCourse3()
        {
            var lessons = new List<Lesson>
            {
                new()
                {
                    LessonCode = "C3-L1",
                    Slug = "why-protected-areas-exist",
                    Title = "Why Protected Marine Areas Exist",
                    NarrativeHtml = "<p>Protected marine areas exist because some habitats and species are especially vulnerable, slow to recover, or vital to ecological resilience. Operational decisions near protected areas require enhanced caution and spatial awareness.</p>",
                    Slides =
                    [
                        Slide("Purpose of Protection", "Protected areas safeguard ecological functions and vulnerable species.", "Biodiversity protection", "Habitat resilience", "Species recovery", "Long-term sustainability"),
                        Slide("Operational Relevance", "Protection status changes how crews plan and monitor operations.", "Know where you are", "Know what is sensitive", "Know what is prohibited"),
                        Slide("Consequences of Harm", "Damage in protected areas can be severe and long-lasting.", "Ecological loss", "Regulatory action", "Reputational damage", "Operational shutdown"),
                        Slide("Prevention Strategy", "The best protection measure is keeping risk away from sensitive areas.", "Route awareness", "Buffer discipline", "Weather awareness", "Early escalation")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C3KC1", "ELC-C3", "C3-L1", "Protected marine areas often exist because the habitats or species involved are especially vulnerable.", true, "Protected status usually reflects ecological sensitivity or conservation importance."),
                        Mcq("C3KC2", "ELC-C3", "C3-L1", "What is the most important first step before operating near a protected area?", "Confirm spatial location and the specific environmental constraints that apply.", 0, "Confirm location and restrictions", "Increase transfer speed", "Reduce recordkeeping", "Rely on memory from previous voyages"),
                        Tf("C3KC3", "ELC-C3", "C3-L1", "Environmental consequences in protected areas may take longer to recover than in less sensitive areas.", true, "Sensitive habitats often recover slowly, if at all, after disturbance.")
                    ]
                },
                new()
                {
                    LessonCode = "C3-L2",
                    Slug = "sensitive-species-and-habitats",
                    Title = "Sensitive Species and Habitats",
                    NarrativeHtml = "<p>Crew should understand the types of receptors that matter: seabirds, marine mammals, nursery grounds, reefs, seagrass, and areas used for feeding, breeding, or migration. Sensitivity can change seasonally.</p>",
                    Slides =
                    [
                        Slide("Environmental Receptors", "Receptors are the species, habitats, and ecological processes that can be harmed.", "Bird colonies", "Marine mammals", "Reefs", "Nursery grounds"),
                        Slide("Seasonal Sensitivity", "The same place can have different risk profiles in different seasons.", "Breeding periods", "Migration windows", "Weather patterns", "Food availability"),
                        Slide("Observation Matters", "Operational watchkeepers can be the first to detect heightened environmental sensitivity.", "Wildlife sightings", "Aggregation behaviour", "Abnormal presence"),
                        Slide("Adjusting Operations", "Observed sensitivity should inform escalation and decision-making.", "Increase caution", "Review controls", "Pause if necessary")
                    ],
                    KnowledgeChecks =
                    [
                        Mcq("C3KC4", "ELC-C3", "C3-L2", "What is an environmental receptor in operational compliance terms?", "A species, habitat, or ecological process that may be harmed.", 2, "A transfer checklist owner", "A weather forecast point", "A species, habitat, or ecological process at risk", "A communications channel"),
                        Tf("C3KC5", "ELC-C3", "C3-L2", "Sensitivity near protected areas can change with breeding or migration seasons.", true, "Seasonal patterns influence how vulnerable an area is at a given time."),
                        Tf("C3KC6", "ELC-C3", "C3-L2", "Wildlife observations during transfer have no operational significance unless pollution is already visible.", false, "Wildlife observations may indicate increased sensitivity and should shape risk decisions.")
                    ]
                },
                new()
                {
                    LessonCode = "C3-L3",
                    Slug = "buffer-zones-and-operational-boundaries",
                    Title = "Buffer Zones and Operational Boundaries",
                    NarrativeHtml = "<p>Buffer zones create separation between activity and sensitivity. Crew should understand that boundaries are not suggestions; they are control measures. Drift, anchoring, tug movement, and weather can all erode the intended buffer.</p>",
                    Slides =
                    [
                        Slide("Purpose of Buffers", "Buffers reduce the probability that normal variation becomes environmental harm.", "Spatial separation", "Decision margin", "Time to react"),
                        Slide("Boundary Erosion", "Operational drift can shrink margins silently.", "Weather changes", "Position drift", "Support vessel movement", "Line load changes"),
                        Slide("Monitoring the Boundary", "Boundary compliance requires active tracking.", "Position awareness", "Trend awareness", "Shared situational awareness"),
                        Slide("Escalation", "When margin shrinks, action should happen early.", "Pause", "Reposition", "Reassess", "Inform control room")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C3KC7", "ELC-C3", "C3-L3", "Buffer zones provide useful reaction time when conditions drift toward a sensitive area.", true, "Buffers help absorb minor variation before an incident occurs."),
                        Mcq("C3KC8", "ELC-C3", "C3-L3", "Which factor can quietly erode an environmental buffer?", "Position drift caused by weather or vessel movement.", 1, "Crew meal schedule", "Position drift and weather", "A completed permit", "Routine paperwork review"),
                        Tf("C3KC9", "ELC-C3", "C3-L3", "Boundaries near protected areas should be treated as advisory only if operations remain efficient.", false, "Boundaries are control measures and must be maintained." )
                    ]
                },
                new()
                {
                    LessonCode = "C3-L4",
                    Slug = "responding-to-heightened-sensitivity",
                    Title = "Responding to Heightened Sensitivity",
                    NarrativeHtml = "<p>When conditions suggest heightened environmental sensitivity—wildlife aggregation, deteriorating weather, or proximity changes—crews should bias toward caution. Early pause and reassessment can prevent irreversible harm.</p>",
                    Slides =
                    [
                        Slide("Heightened Sensitivity", "Conditions can worsen even without a spill.", "Wildlife density", "Changing drift", "Reduced visibility", "Support limitations"),
                        Slide("Bias to Caution", "Environmental protection requires conservative decision-making.", "Pause early", "Reassess scope", "Do not rationalise"),
                        Slide("Communication", "Environmental concerns must be shared fast and clearly.", "Bridge", "POAC", "Control room", "Support craft"),
                        Slide("Decision Quality", "Good decisions protect both the environment and the operation.", "Preserve margins", "Preserve evidence", "Preserve confidence")
                    ],
                    KnowledgeChecks =
                    [
                        Mcq("C3KC10", "ELC-C3", "C3-L4", "What should crews do when wildlife density increases close to the transfer area?", "Escalate and reassess the operation conservatively.", 3, "Ignore it until pollution is seen", "Reduce logging frequency", "Wait until the transfer ends", "Escalate and reassess conservatively"),
                        Tf("C3KC11", "ELC-C3", "C3-L4", "Early pause is often the best option when environmental sensitivity increases unexpectedly.", true, "Pausing early preserves control and protects sensitive receptors."),
                        Tf("C3KC12", "ELC-C3", "C3-L4", "If no product has leaked, environmental observations never need escalation.", false, "Heightened sensitivity can itself justify escalation and control adjustment.")
                    ]
                }
            };

            return new Course
            {
                CourseCode = "ELC-C3",
                Slug = "protected-marine-areas",
                Title = "Course 3: Protected Marine Areas",
                Overview = "Builds operational awareness of protected areas, ecological sensitivity, buffer control, and conservative decision-making near vulnerable habitats.",
                RegulatoryReference = "Protected area restrictions, site sensitivity controls, environmental watch and escalation expectations.",
                Audience = "Crew operating near sensitive coastal and offshore habitats.",
                EstimatedMinutes = 85,
                Lessons = lessons,
                Flashcards =
                [
                    Flashcard("Protected areas", "Why they exist", "To protect vulnerable habitats, species, and ecological functions."),
                    Flashcard("Sensitivity", "Environmental receptor", "Any habitat, species, or ecological process that can be harmed."),
                    Flashcard("Spatial control", "Buffer zone", "A safety margin between operations and sensitive areas."),
                    Flashcard("Observation", "Wildlife aggregation", "A warning sign that sensitivity may be elevated and controls may need review."),
                    Flashcard("Decision bias", "Conservative action", "When sensitivity grows, choose the safer option early."),
                    Flashcard("Boundary management", "Margin erosion", "Weather and vessel drift can shrink a buffer without anyone noticing unless it is monitored." )
                ],
                ScenarioTasks = [BuildCourse3Scenario()],
                QuestionBank = BuildCourse3Questions()
            };
        }

        private static Course BuildCourse4()
        {
            var lessons = new List<Lesson>
            {
                new()
                {
                    LessonCode = "C4-L1",
                    Slug = "crew-environmental-duties",
                    Title = "Crew Environmental Duties",
                    NarrativeHtml = "<p>Environmental responsibility is shared. Deck crew, engineers, officers, and masters all influence whether pollution is prevented, detected, and managed well. Clear task ownership is essential, but ownership never removes everyone else’s duty to act.</p>",
                    Slides =
                    [
                        Slide("Shared Responsibility", "Environmental protection is a whole-crew outcome.", "Everyone observes", "Everyone reports", "Everyone follows controls"),
                        Slide("Role Clarity", "Clear duties improve response quality.", "Watchkeeping", "Equipment checks", "Communications", "Evidence capture"),
                        Slide("If You See Risk", "Silence increases environmental exposure.", "Challenge", "Pause", "Escalate", "Record"),
                        Slide("Professional Standard", "Environmental care is part of seamanship and operational professionalism.", "Discipline", "Accuracy", "Vigilance")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C4KC1", "ELC-C4", "C4-L1", "Environmental responsibility is shared even when individual tasks are allocated to specific roles.", true, "Task allocation does not remove the duty to speak up or act appropriately."),
                        Mcq("C4KC2", "ELC-C4", "C4-L1", "What should a crew member do when they see an environmental control being bypassed?", "Challenge, pause if required, and escalate using the agreed process.", 2, "Ignore it if a supervisor is nearby", "Record it later if there is time", "Challenge and escalate through the proper chain", "Wait for an audit to raise it"),
                        Tf("C4KC3", "ELC-C4", "C4-L1", "Only the person assigned to environmental watch should report wildlife or pollution concerns.", false, "Anyone observing a concern must report it promptly.")
                    ]
                },
                new()
                {
                    LessonCode = "C4-L2",
                    Slug = "housekeeping-waste-and-pollution-prevention",
                    Title = "Housekeeping, Waste, and Pollution Prevention",
                    NarrativeHtml = "<p>Environmental compliance is strengthened by disciplined housekeeping, segregated waste, proper storage, drip control, and clean working decks. Small leaks, unsecured absorbents, and poor storage can become real releases in heavy weather.</p>",
                    Slides =
                    [
                        Slide("Housekeeping as Control", "Clean work areas reduce the probability of accidental release.", "Clean decks", "Secure stores", "Label containers", "Check drains"),
                        Slide("Waste Discipline", "Improper waste handling creates environmental and legal exposure.", "Segregate correctly", "Store safely", "Dispose through approved channels"),
                        Slide("Small Leak Mindset", "Minor leaks are warning signs, not inconveniences.", "Contain quickly", "Trace the source", "Record and escalate"),
                        Slide("Weather Factor", "Sea state can turn a minor defect into a release event.", "Secure early", "Inspect often", "Act before conditions worsen")
                    ],
                    KnowledgeChecks =
                    [
                        Mcq("C4KC4", "ELC-C4", "C4-L2", "Why is disciplined housekeeping part of environmental compliance?", "Because poor housekeeping increases the chance of accidental release and weakens response effectiveness.", 1, "Because it is mainly a cosmetic standard", "Because it reduces accidental release risk", "Because it replaces permit controls", "Because it removes the need for monitoring"),
                        Tf("C4KC5", "ELC-C4", "C4-L2", "Minor leaks should be treated as warning signals and investigated.", true, "Minor leaks often indicate underlying control weakness."),
                        Tf("C4KC6", "ELC-C4", "C4-L2", "Waste segregation is optional if the vessel is operating offshore.", false, "Waste handling rules still apply and poor segregation can create environmental exposure.")
                    ]
                },
                new()
                {
                    LessonCode = "C4-L3",
                    Slug = "watchkeeping-and-incident-response",
                    Title = "Watchkeeping and Incident Response",
                    NarrativeHtml = "<p>Effective watchkeeping supports early detection. Incident response starts with recognition, communication, containment, and evidence capture. Crew must understand the first minutes matter most for limiting impact.</p>",
                    Slides =
                    [
                        Slide("Watchkeeping Value", "The earlier a problem is detected, the more options remain available.", "Spot trends", "Confirm alarms", "Report changes", "Protect margins"),
                        Slide("First Response", "Initial actions should be immediate and disciplined.", "Stop source if safe", "Alert chain of command", "Deploy first barriers", "Protect people"),
                        Slide("Evidence and Facts", "Good response includes good documentation.", "Time", "Position", "Conditions", "Actions taken"),
                        Slide("Recovery", "Stabilise first, then investigate fully.", "Contain", "Communicate", "Record", "Review")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C4KC7", "ELC-C4", "C4-L3", "The first minutes of an environmental incident are often critical for limiting impact.", true, "Early action can materially reduce environmental spread and severity."),
                        Mcq("C4KC8", "ELC-C4", "C4-L3", "Which action belongs in the immediate response phase when safe to perform?", "Stopping the source and alerting the chain of command.", 0, "Stopping the source and alerting command", "Rewriting log entries for clarity", "Waiting for shore approval before reporting internally", "Suspending all recordkeeping"),
                        Tf("C4KC9", "ELC-C4", "C4-L3", "Evidence collection should wait until the incident has completely ended.", false, "Evidence should be preserved as early as safely possible.")
                    ]
                },
                new()
                {
                    LessonCode = "C4-L4",
                    Slug = "leadership-learning-and-compliance-culture",
                    Title = "Leadership, Learning, and Compliance Culture",
                    NarrativeHtml = "<p>Environmental performance improves when leaders model discipline, crew feel safe to speak up, and the organisation learns from weak signals, deviations, and near misses. A compliance culture is measurable in daily habits.</p>",
                    Slides =
                    [
                        Slide("Compliance Culture", "Culture is what crews do when conditions become difficult.", "Do they speak up?", "Do they pause?", "Do they record accurately?"),
                        Slide("Leadership Behaviour", "Leaders set the tone for environmental performance.", "Invite challenge", "Reward reporting", "Avoid blame-driven silence"),
                        Slide("Learning System", "Every near miss is an opportunity to strengthen controls.", "Review", "Train", "Update procedures", "Share lessons"),
                        Slide("End State", "Strong environmental culture creates reliable operations.", "Fewer surprises", "Better evidence", "Stronger trust")
                    ],
                    KnowledgeChecks =
                    [
                        Tf("C4KC10", "ELC-C4", "C4-L4", "A blame-heavy culture can reduce reporting quality and weaken environmental compliance.", true, "People are less likely to escalate issues when they expect punishment for speaking up."),
                        Mcq("C4KC11", "ELC-C4", "C4-L4", "Which leadership behaviour best supports environmental compliance?", "Encouraging early reporting and constructive challenge.", 2, "Prioritising schedule over control", "Discouraging near-miss reporting", "Encouraging early reporting and challenge", "Keeping lessons learned within management only"),
                        Tf("C4KC12", "ELC-C4", "C4-L4", "Near misses should be used to strengthen training and procedures.", true, "Learning from weak signals is a hallmark of high-reliability operations.")
                    ]
                }
            };

            return new Course
            {
                CourseCode = "ELC-C4",
                Slug = "environmental-responsibilities-of-crew",
                Title = "Course 4: Environmental Responsibilities of Crew",
                Overview = "Turns environmental compliance into concrete behaviours for every crew role: housekeeping, watchkeeping, incident response, reporting, and leadership.",
                RegulatoryReference = "Crew duty-of-care practices, shipboard pollution prevention, environmental reporting discipline.",
                Audience = "All shipboard personnel and supervisory staff.",
                EstimatedMinutes = 80,
                Lessons = lessons,
                Flashcards =
                [
                    Flashcard("Shared duty", "Everyone has a role", "Environmental protection depends on observation, reporting, and disciplined execution by the whole crew."),
                    Flashcard("Housekeeping", "Clean deck = safer deck", "Good housekeeping reduces accidental release pathways and improves response readiness."),
                    Flashcard("Watchkeeping", "Early detection", "Early detection protects margins and reduces environmental consequence."),
                    Flashcard("Response", "First minutes matter", "Contain, communicate, and document immediately when an incident occurs."),
                    Flashcard("Leadership", "Speak-up culture", "Crews must feel able to raise environmental concerns without delay."),
                    Flashcard("Learning", "Near misses are data", "Use deviations to improve training, procedures, and controls.")
                ],
                ScenarioTasks = [BuildCourse4Scenario()],
                QuestionBank = BuildCourse4Questions()
            };
        }

        private static CompetencyTask BuildCourse1Scenario() => new()
        {
            TaskId = "C1-S1",
            Title = "Forecasted Weather Deterioration Before Transfer",
            Description = "Assess whether the trainee applies duty of care and escalation principles before the transfer begins.",
            SuccessCriteria = "Pause early, escalate, preserve evidence of the decision basis, and avoid normalising deviation.",
            Steps =
            [
                Step("c1s1-step1", "Pre-start briefing", "A new weather update shows the sea state may exceed the approved limit during the transfer window. What should the team do first?", null, "Use conservative decision-making.",
                    Choice("hold", "Pause the start and escalate the updated conditions.", "c1s1-step2", true, 1m, "Correct. Duty of care requires reassessment before risk increases."),
                    Choice("proceed", "Proceed because the job has already been prepared.", null, false, 0m, "Incorrect. Preparation does not override changing environmental risk.")),
                Step("c1s1-step2", "Decision record", "What should be captured in the record?", null, "Think evidence and chronology.",
                    Choice("facts", "The forecast change, timing, people informed, and decision taken.", null, true, 1m, "Correct. Factual, timely records create legal traceability."),
                    Choice("minimal", "Only that the transfer was delayed.", null, false, 0m, "Too vague. The basis for the decision matters."))
            ]
        };

        private static CompetencyTask BuildCourse2Scenario() => new()
        {
            TaskId = "C2-S1",
            Title = "Pressure Anomaly During Offshore Transfer",
            Description = "Test whether the trainee responds to a transfer control deviation in line with DFFE expectations.",
            SuccessCriteria = "Pause transfer, investigate the anomaly, communicate clearly, and only resume when the envelope is restored.",
            Steps =
            [
                Step("c2s1-step1", "Transfer monitoring", "Mid-transfer, manifold pressure begins fluctuating beyond the normal trend. What is the best immediate action?", null, "Use pause criteria, not optimism.",
                    Choice("pause", "Pause transfer and escalate the anomaly for assessment.", "c2s1-step2", true, 1m, "Correct. A growing anomaly is an escalation trigger."),
                    Choice("watch", "Continue while monitoring for a few more minutes.", null, false, 0m, "Incorrect. Waiting can allow the condition to worsen.")),
                Step("c2s1-step2", "Investigation", "Which follow-up action supports compliance?", null, "Think control and traceability.",
                    Choice("record", "Record the anomaly, actions taken, and communication chronology.", null, true, 1m, "Correct. The response should be both controlled and documented."),
                    Choice("resume", "Resume as soon as pressure stabilises without documenting the event.", null, false, 0m, "Incorrect. Closeout records are part of compliant control.") )
            ]
        };

        private static CompetencyTask BuildCourse3Scenario() => new()
        {
            TaskId = "C3-S1",
            Title = "Wildlife Aggregation Near a Sensitive Area",
            Description = "Evaluate whether the trainee recognises heightened sensitivity and adopts conservative action.",
            SuccessCriteria = "Escalate wildlife density changes, reassess the boundary, and bias toward caution.",
            Steps =
            [
                Step("c3s1-step1", "Environmental observation", "During set-up near a protected area, observers report a growing aggregation of seabirds. What is the best response?", null, "Heightened sensitivity should affect the decision.",
                    Choice("escalate", "Escalate and reassess whether the operation should proceed.", "c3s1-step2", true, 1m, "Correct. Unexpected receptor sensitivity should shape operational decisions."),
                    Choice("ignore", "Continue because no release has occurred.", null, false, 0m, "Incorrect. Environmental sensitivity alone can justify reassessment.")),
                Step("c3s1-step2", "Boundary review", "What should be reviewed next?", null, "Think spatial margin and operational drift.",
                    Choice("buffer", "Buffer margin, weather trend, and position control.", null, true, 1m, "Correct. These factors determine whether the risk remains acceptable."),
                    Choice("schedule", "Only the schedule impact of a delay.", null, false, 0m, "Incorrect. Schedule is secondary to environmental protection.") )
            ]
        };

        private static CompetencyTask BuildCourse4Scenario() => new()
        {
            TaskId = "C4-S1",
            Title = "Small Leak on Deck Before Weather Deteriorates",
            Description = "Assess whether the trainee treats minor leaks as warning signals and acts before conditions worsen.",
            SuccessCriteria = "Contain early, trace the source, escalate through the chain, and document the event.",
            Steps =
            [
                Step("c4s1-step1", "Deck inspection", "A small persistent drip is found near transfer equipment before heavy weather is expected. What should the crew do first?", null, "Minor defects can become major releases.",
                    Choice("contain", "Contain the leak, stop the source if safe, and escalate.", "c4s1-step2", true, 1m, "Correct. Early action prevents escalation."),
                    Choice("delay", "Ignore it because absorbent pads are already nearby.", null, false, 0m, "Incorrect. Absorbents do not remove the need to address the source.")),
                Step("c4s1-step2", "Learning and closeout", "What should happen after the leak is controlled?", null, "Think traceability and learning.",
                    Choice("record", "Record the defect, response, and corrective action for follow-up.", null, true, 1m, "Correct. Near misses and defects should strengthen future control."),
                    Choice("forget", "Return to routine work without logging because no spill reached the sea.", null, false, 0m, "Incorrect. Weak signals still matter.") )
            ]
        };

        private static CompetencyTask BuildModuleCompetencyTask() => new()
        {
            TaskId = "M1-T1",
            Title = "Module-Level Environmental Decision Drill",
            Description = "A combined scenario that tests legal awareness, protected-area sensitivity, and operational stop-work judgment.",
            SuccessCriteria = "Recognise a changing risk picture, pause before the threshold is lost, and document the control decision.",
            Steps =
            [
                Step("m1t1-step1", "Changing conditions", "A transfer is planned near a sensitive area. Weather is trending worse and wildlife presence is increasing. What should happen?", null, "Choose the control-preserving option.",
                    Choice("pause", "Pause planning and reassess controls before continuing.", "m1t1-step2", true, 1m, "Correct. This preserves margin and defensibility."),
                    Choice("continue", "Continue because product has not yet moved.", null, false, 0m, "Incorrect. Foreseeable environmental risk must be controlled before work starts.")),
                Step("m1t1-step2", "Evidence", "What should be retained after the reassessment decision?", null, "Think audit trail.",
                    Choice("audit", "Forecast data, sightings, communications, and decision records.", null, true, 1m, "Correct. Strong evidence supports traceability."),
                    Choice("none", "No special records are needed if the transfer is postponed.", null, false, 0m, "Incorrect. Decision basis should be documented.") )
            ]
        };

        private static CompetencyTask BuildEscalationCompetencyTask() => new()
        {
            TaskId = "M1-T2",
            Title = "Module-Level Escalation and Reporting Drill",
            Description = "A combined scenario focused on reporting quality and command escalation during an evolving environmental threat.",
            SuccessCriteria = "Use factual escalation, protect evidence, and maintain clear chronology.",
            Steps =
            [
                Step("m1t2-step1", "Initial notification", "A sheen is suspected but not yet confirmed. What should the watchkeeper communicate first?", null, "Use facts, not assumptions.",
                    Choice("facts", "Observed conditions, location, time, and immediate control actions.", "m1t2-step2", true, 1m, "Correct. Factual communication enables effective response."),
                    Choice("guess", "A likely cause and estimated legal outcome.", null, false, 0m, "Incorrect. Initial reports should be factual and avoid speculation.")),
                Step("m1t2-step2", "Evidence protection", "What should the team protect immediately?", null, "Think records and chronology.",
                    Choice("records", "Logs, photos, equipment status, positions, and witness observations.", null, true, 1m, "Correct. Preserve the evidence chain early."),
                    Choice("cleanup", "Only the cleanup equipment, not the evidence trail.", null, false, 0m, "Incorrect. Evidence protection is part of effective incident management.") )
            ]
        };

        private static IReadOnlyList<Question> BuildCourse1Questions() =>
        [
            Mcq("C1Q01", "ELC-C1", "C1-L1", "What is the main purpose of environmental law in offshore operations?", "Environmental law sets enforceable duties that prevent harm and shape response behaviour.", 1, "To improve voyage speed", "To prevent harm and define duties", "To replace watchkeeping", "To reduce fuel consumption"),
            Mcq("C1Q02", "ELC-C1", "C1-L1", "Which phrase best describes duty of care?", "Duty of care requires reasonable preventive and corrective action.", 2, "Do nothing until shore instructs", "Follow custom even if risks change", "Take reasonable steps to prevent and minimise harm", "Only act after a spill reaches shore"),
            Mcq("C1Q03", "ELC-C1", "C1-L2", "Which source can create binding environmental obligations for a crew?", "Permit conditions are binding operational obligations.", 0, "Permit conditions", "Informal preferences", "Rumours from previous crews", "Unverified assumptions"),
            Mcq("C1Q04", "ELC-C1", "C1-L2", "If conditions fall outside approved environmental controls, what should happen?", "Operations should pause and be reassessed.", 3, "Continue but note it later", "Reduce communications", "Ignore it if no pollution is visible", "Pause and escalate"),
            Mcq("C1Q05", "ELC-C1", "C1-L3", "What type of report is most defensible after an incident?", "A factual, chronological report supports legal defensibility.", 1, "A speculative report", "A factual chronological report", "A verbal-only report", "A report delayed until all evidence is gone"),
            Mcq("C1Q06", "ELC-C1", "C1-L3", "Which item should be preserved as evidence?", "Timestamps and original records are important evidence sources.", 2, "Only personal opinions", "Only equipment manuals", "Logs, timestamps, and photos", "Only final summary notes"),
            Mcq("C1Q07", "ELC-C1", "C1-L4", "What does strong compliance culture look like?", "Strong culture shows up in challenge, pause, and accurate records.", 0, "Challenge, pause, and accurate records", "Silence and speed", "Minimal logs", "Avoiding near-miss reporting"),
            Mcq("C1Q08", "ELC-C1", "C1-L4", "What is a weak signal of compliance drift?", "Rushed checklists are a known weak signal.", 3, "Clear communication", "Accurate handovers", "Prompt escalation", "Rushed checklists"),
            Mcq("C1Q09", "ELC-C1", "C1-L1", "Why should crews know reporting triggers before an operation starts?", "Knowing triggers supports timely and accurate escalation.", 2, "To reduce the number of reports", "To avoid checklists", "To support timely escalation", "To remove accountability"),
            Mcq("C1Q10", "ELC-C1", "C1-L2", "When uncertainty exists about environmental risk, which option is best?", "The safer and more protective option is best when uncertainty exists.", 1, "The fastest option", "The safer protective option", "The least documented option", "The cheapest option"),
            Tf("C1Q11", "ELC-C1", "C1-L1", "Environmental law matters only after pollution has already occurred.", false, "Environmental law is strongly preventive and applies before incidents occur."),
            Tf("C1Q12", "ELC-C1", "C1-L1", "Environmental law can shape how operations are planned and monitored.", true, "It affects planning, controls, monitoring, and reporting."),
            Tf("C1Q13", "ELC-C1", "C1-L2", "Duty of care applies even when the exact scale of harm is still uncertain.", true, "Reasonable preventive steps are required when risks are foreseeable."),
            Tf("C1Q14", "ELC-C1", "C1-L2", "If no spill has occurred yet, control deviations are legally irrelevant.", false, "Control deviations are important because they increase foreseeable risk."),
            Tf("C1Q15", "ELC-C1", "C1-L3", "Good reporting should avoid speculation and focus on facts.", true, "Factual reporting is more reliable and defensible."),
            Tf("C1Q16", "ELC-C1", "C1-L3", "Evidence preservation can wait until after all cleanup work is finished.", false, "Evidence should be preserved as early as safely possible."),
            Tf("C1Q17", "ELC-C1", "C1-L4", "Accurate handovers support environmental compliance.", true, "Handover quality directly affects situational awareness and control."),
            Tf("C1Q18", "ELC-C1", "C1-L4", "Only senior officers are responsible for escalating environmental concerns.", false, "Any crew member observing an environmental concern should escalate through the proper process."),
            Tf("C1Q19", "ELC-C1", "C1-L3", "Unexpected leak indications should normally trigger immediate attention.", true, "Leak indications are high-value warning signs."),
            Tf("C1Q20", "ELC-C1", "C1-L2", "Permit conditions may be ignored if they slow the operation down.", false, "Permit conditions are part of binding compliance controls."),
            Tf("C1Q21", "ELC-C1", "C1-L1", "Environmental law often protects shared public and ecological interests.", true, "Marine protection measures often support the public good and long-term sustainability."),
            Tf("C1Q22", "ELC-C1", "C1-L4", "Unreported near misses can hide important control weaknesses.", true, "Near misses are valuable indicators of system weakness."),
            Tf("C1Q23", "ELC-C1", "C1-L2", "If a hazard is foreseeable, reasonable control action is expected.", true, "Foreseeable risk should be managed before work continues."),
            Tf("C1Q24", "ELC-C1", "C1-L3", "Incident chronology is less important than speed when documenting events.", false, "Chronology is essential for traceability and understanding response effectiveness."),
            Tf("C1Q25", "ELC-C1", "C1-L4", "Operational excellence and environmental compliance often reinforce one another.", true, "Discipline and control support both safety and environmental outcomes."),
            Tf("C1Q26", "ELC-C1", "C1-L3", "Logs should be altered after the event if it improves readability.", false, "Original records should be preserved; corrections must be transparent."),
            Tf("C1Q27", "ELC-C1", "C1-L4", "Weak communication can be an early sign of larger compliance failure.", true, "Communication breakdowns often precede control loss."),
            Tf("C1Q28", "ELC-C1", "C1-L1", "Environmental law is irrelevant to routine offshore work.", false, "Routine work is where compliance is most often demonstrated."),
            ScenarioQuestion("C1Q29", "ELC-C1", "C1-L2", "A forecast update shows conditions may exceed the approved limit during transfer. What should happen first?", "The team must decide whether to continue mobilising or pause.", "Pause and escalate the updated conditions.", 0, "Pause and escalate the updated conditions.", "Proceed because preparations are complete.", "Wait until the transfer actually starts.", "Ignore it unless shore raises the issue."),
            ScenarioQuestion("C1Q30", "ELC-C1", "C1-L3", "A crew member sees a faint sheen but is unsure whether it is product. What is the best immediate response?", "The first report should avoid speculation but preserve urgency.", "Report the observation factually and initiate checks.", 1, "Ignore it until confirmed.", "Report the observation factually and initiate checks.", "Rewrite the log after confirmation.", "Wait for daylight before mentioning it."),
            ScenarioQuestion("C1Q31", "ELC-C1", "C1-L4", "Checklist quality is deteriorating because the crew feels rushed. What is the best compliance response?", "Schedule pressure is not a valid reason to weaken controls.", "Pause and restore control discipline.", 2, "Accept the reduced quality temporarily.", "Finish quickly and improve later.", "Pause and restore control discipline.", "Remove non-essential records."),
            ScenarioQuestion("C1Q32", "ELC-C1", "C1-L1", "A junior rating sees a hose drip but assumes only supervisors should act. What is the correct principle?", "Any person who sees risk should escalate it through the proper process.", "Escalate regardless of rank.", 0, "Escalate regardless of rank.", "Wait for a supervisor to notice.", "Only record it after the operation.", "Keep silent if the leak is small."),
            ScenarioQuestion("C1Q33", "ELC-C1", "C1-L3", "Post-incident, a colleague suggests simplifying the log by removing uncertain details. What should you do?", "Original facts and uncertainty should be preserved honestly.", "Keep the original chronology and mark uncertainty clearly.", 2, "Rewrite everything from memory.", "Delete uncertain details.", "Keep the original chronology and mark uncertainty clearly.", "Wait until the investigation ends."),
            ScenarioQuestion("C1Q34", "ELC-C1", "C1-L2", "A control deviation is discovered but no one wants to delay cargo. Which option best reflects duty of care?", "Environmental duty of care favours control over convenience.", "Pause and assess the deviation before continuing.", 3, "Continue and note it later.", "Lower the reporting threshold.", "Ask only the most senior person present.", "Pause and assess the deviation before continuing."),
            ScenarioQuestion("C1Q35", "ELC-C1", "C1-L4", "A near miss occurs with no spill. What should happen next?", "Near misses should feed learning and control improvement.", "Record and review it as a learning opportunity.", 1, "Forget it because there was no impact.", "Record and review it as a learning opportunity.", "Keep it informal.", "Delay reporting until audit season."),
            ScenarioQuestion("C1Q36", "ELC-C1", "C1-L1", "During a toolbox talk, someone says environmental law is a shore problem. What should the facilitator clarify?", "Environmental law shapes shipboard planning and execution too.", "It directly affects shipboard planning and execution.", 0, "It directly affects shipboard planning and execution.", "It only matters in court.", "It is mainly for company managers.", "It applies after the voyage only."),
            ScenarioQuestion("C1Q37", "ELC-C1", "C1-L3", "After a suspected incident, what set of evidence is most useful?", "A strong evidence set includes positions, photos, timestamps, and actions taken.", "Photos, positions, timestamps, and actions taken.", 2, "Only witness opinions.", "Only final summary emails.", "Photos, positions, timestamps, and actions taken.", "Only weather data."),
            ScenarioQuestion("C1Q38", "ELC-C1", "C1-L4", "An officer discourages reporting because it may trigger scrutiny. What should happen?", "Compliance culture depends on encouraging early reporting.", "Challenge that behaviour and escalate appropriately.", 1, "Accept the instruction quietly.", "Challenge that behaviour and escalate appropriately.", "Delete the draft report.", "Wait for the end of the voyage."),
            ScenarioQuestion("C1Q39", "ELC-C1", "C1-L2", "A team is unsure whether a planned step falls within permit conditions. What is the correct next move?", "Clarify before proceeding.", "Stop and clarify the permit position before proceeding.", 3, "Proceed because the step seems minor.", "Let the next watch decide.", "Skip the step silently.", "Stop and clarify before proceeding."),
            ScenarioQuestion("C1Q40", "ELC-C1", "C1-L4", "What best describes a high-reliability environmental culture?", "High reliability combines discipline, challenge, reporting, and learning.", "Discipline, challenge, accurate reporting, and learning.", 0, "Discipline, challenge, accurate reporting, and learning.", "Speed and silence.", "Minimal logs and low visibility.", "Management-only responsibility.")
        ];

        private static IReadOnlyList<Question> BuildCourse2Questions() =>
        [
            Mcq("C2Q01", "ELC-C2", "C2-L1", "What is the core purpose of DFFE offshore bunkering controls?", "They manage environmental risk by defining authorised operating conditions.", 2, "To simplify logistics", "To increase vessel speed", "To manage environmental risk through controlled conditions", "To remove the need for shipboard procedures"),
            Mcq("C2Q02", "ELC-C2", "C2-L1", "What does the authorised operating envelope include?", "The envelope includes approved location, conditions, and controls.", 0, "Approved location, conditions, and controls", "Only cargo quantity", "Only company preference", "Only weather forecast history"),
            Mcq("C2Q03", "ELC-C2", "C2-L2", "Which check belongs before transfer begins?", "Weather confirmation is part of pre-transfer environmental readiness.", 1, "Post-transfer debrief", "Weather and readiness confirmation", "Final incident summary", "Crew leave planning"),
            Mcq("C2Q04", "ELC-C2", "C2-L2", "Why are communications checks part of environmental compliance?", "Reliable communications support control and fast escalation.", 3, "They are optional if visibility is good", "They matter only after a spill", "They are purely administrative", "They support control and escalation"),
            Mcq("C2Q05", "ELC-C2", "C2-L3", "How should crews treat a growing trend away from the safe envelope?", "Escalate early before the trend becomes an incident.", 2, "Ignore it if transfer is on schedule", "Wait until completion", "Escalate early", "Reduce watchkeeping"),
            Mcq("C2Q06", "ELC-C2", "C2-L3", "What is a pause during transfer best understood as?", "A pause is a control measure that protects the operation.", 1, "A sign of weak leadership", "A control measure", "A breach in itself", "A paperwork issue"),
            Mcq("C2Q07", "ELC-C2", "C2-L4", "What should compliant closeout include?", "It should include deviations and environmental observations, not just completion time.", 0, "Completion details, deviations, and observations", "Only the end time", "Only the cargo volume", "Nothing if no spill occurred"),
            Mcq("C2Q08", "ELC-C2", "C2-L4", "Why do near misses matter after bunkering?", "They reveal weak barriers before the next operation.", 3, "They reduce recordkeeping", "They replace maintenance", "They end accountability", "They reveal weak barriers"),
            Mcq("C2Q09", "ELC-C2", "C2-L1", "Which statement about permit conditions is correct?", "Permit conditions should be treated as binding operating controls.", 2, "They are optional guidance", "They only apply on paper", "They are binding operating controls", "They can be overridden informally"),
            Mcq("C2Q10", "ELC-C2", "C2-L3", "Which is an example of continuous verification?", "Rechecking position, behaviour, and environmental conditions during transfer.", 1, "Checking only once before start", "Rechecking conditions throughout transfer", "Only reviewing the permit after completion", "Pausing all observations during stable weather"),
            Tf("C2Q11", "ELC-C2", "C2-L1", "DFFE controls can affect both preparation and execution of offshore transfer.", true, "Preparation and execution are both part of compliant control."),
            Tf("C2Q12", "ELC-C2", "C2-L1", "If a vessel is technically ready, environmental readiness no longer matters.", false, "Environmental readiness remains essential even when the vessel is technically ready."),
            Tf("C2Q13", "ELC-C2", "C2-L2", "Pre-transfer environmental readiness may include weather checks and watch assignments.", true, "Both are part of a credible readiness review."),
            Tf("C2Q14", "ELC-C2", "C2-L2", "Wildlife watch requirements can be decided after transfer begins.", false, "They should be briefed before the operation starts."),
            Tf("C2Q15", "ELC-C2", "C2-L3", "Pressure anomalies can be an escalation trigger during transfer.", true, "They may indicate loss-of-control developing."),
            Tf("C2Q16", "ELC-C2", "C2-L3", "Minor communications degradation is acceptable if product is already flowing.", false, "Reliable communication is a core control throughout transfer."),
            Tf("C2Q17", "ELC-C2", "C2-L4", "Closeout records should reflect abnormal observations even when no release occurred.", true, "Deviations and near misses still matter."),
            Tf("C2Q18", "ELC-C2", "C2-L4", "Near misses are only useful for insurance teams, not operations.", false, "They are highly valuable for operational learning."),
            Tf("C2Q19", "ELC-C2", "C2-L3", "Operational drift often begins with acceptance of small unresolved problems.", true, "Normalising weak signals erodes control discipline."),
            Tf("C2Q20", "ELC-C2", "C2-L1", "Authorised conditions can usually be adapted on the fly without approval if efficiency improves.", false, "Authorised conditions define the lawful operating envelope."),
            Tf("C2Q21", "ELC-C2", "C2-L2", "Communication tests should happen before product movement starts.", true, "They verify the control chain before exposure begins."),
            Tf("C2Q22", "ELC-C2", "C2-L2", "Marginal conditions are a good reason to compress pre-transfer checks.", false, "Marginal conditions demand more caution, not less."),
            Tf("C2Q23", "ELC-C2", "C2-L3", "A transfer pause can protect legal defensibility as well as the environment.", true, "Pausing shows control, caution, and barrier discipline."),
            Tf("C2Q24", "ELC-C2", "C2-L4", "Post-transfer debriefs can improve future environmental performance.", true, "Debriefs support the learning loop."),
            Tf("C2Q25", "ELC-C2", "C2-L1", "Environmental control obligations end once the permit is issued.", false, "Issuance is only the start; compliance depends on execution."),
            Tf("C2Q26", "ELC-C2", "C2-L3", "Unexpected movement of vessels or equipment can threaten the safe envelope.", true, "Movement trends can erode control margins."),
            Tf("C2Q27", "ELC-C2", "C2-L4", "Chronological reporting is useful for demonstrating control after transfer deviations.", true, "Chronology helps explain what happened and what was done."),
            Tf("C2Q28", "ELC-C2", "C2-L2", "Emergency equipment readiness should be confirmed before transfer begins.", true, "Readiness is part of the pre-transfer gate."),
            ScenarioQuestion("C2Q29", "ELC-C2", "C2-L2", "Communications with the receiving vessel become intermittent just before transfer. What is the best decision?", "Control barriers are not fully ready.", "Pause and restore reliable communications first.", 0, "Pause and restore reliable communications first.", "Proceed because the vessels remain visible.", "Let the issue ride for the first ten minutes.", "Document it after transfer."),
            ScenarioQuestion("C2Q30", "ELC-C2", "C2-L3", "A manifold pressure trend starts fluctuating beyond the usual range. What should happen?", "A growing anomaly requires active control.", "Pause and investigate the anomaly.", 1, "Ignore it because no spill is visible.", "Pause and investigate the anomaly.", "Reduce lookout frequency.", "Continue unless shore objects."),
            ScenarioQuestion("C2Q31", "ELC-C2", "C2-L3", "A crew member sees a faint sheen during transfer. What is the best first report?", "The first report should be factual and immediate.", "Report the observation factually and trigger checks.", 2, "Wait to see if it grows.", "Assume it is harmless reflection.", "Report the observation factually and trigger checks.", "Only tell the next watch."),
            ScenarioQuestion("C2Q32", "ELC-C2", "C2-L4", "Transfer completed successfully but a small anomaly occurred mid-operation. What should closeout include?", "Deviations belong in the closeout trail.", "Record the anomaly, action taken, and outcome.", 3, "Nothing because the transfer finished.", "Only cargo totals.", "Only final signatures.", "Record the anomaly, action taken, and outcome."),
            ScenarioQuestion("C2Q33", "ELC-C2", "C2-L2", "Weather worsens toward the edge of the permit limit during pre-start checks. What is best?", "Conditions approaching the limit demand conservative control.", "Reassess and delay if the margin is no longer comfortable.", 1, "Proceed because the permit still exists.", "Reassess and delay if margin is weak.", "Skip final checks to beat the weather.", "Transfer the decision to the next shift."),
            ScenarioQuestion("C2Q34", "ELC-C2", "C2-L1", "A crew member says permit conditions are just paperwork. What should be clarified?", "Permit conditions define the authorised operating envelope.", "They are binding operational controls.", 0, "They are binding operational controls.", "They matter only to shore managers.", "They can be ignored if the master agrees.", "They apply only after an incident."),
            ScenarioQuestion("C2Q35", "ELC-C2", "C2-L3", "Unexpected vessel movement begins reducing separation margin. What should the team do?", "Shrinking separation is a real control threat.", "Pause and restore the safe operating envelope.", 2, "Carry on if hoses look stable.", "Note it in the debrief only.", "Pause and restore the safe envelope.", "Switch off environmental watch."),
            ScenarioQuestion("C2Q36", "ELC-C2", "C2-L4", "After transfer, a supervisor suggests leaving out a near miss to avoid attention. What is correct?", "Near misses must support learning and defensible closeout.", "Record the near miss accurately.", 3, "Leave it out if no spill occurred.", "Mention it verbally only.", "Save it for the next audit.", "Record it accurately."),
            ScenarioQuestion("C2Q37", "ELC-C2", "C2-L2", "An environmental watch has not been assigned but all equipment is ready. What now?", "A key control is missing.", "Do not start until the watch arrangement is in place.", 1, "Start because equipment readiness is enough.", "Do not start until the watch is in place.", "Shorten the watch requirement.", "Assign watch after the first hour."),
            ScenarioQuestion("C2Q38", "ELC-C2", "C2-L3", "A transfer alarm clears after a few seconds. What is best practice?", "Transient alarms still deserve assessment if they relate to containment or control.", "Investigate and confirm the system is stable before continuing normally.", 0, "Investigate and confirm stability.", "Ignore it once it clears.", "Mute similar alarms for the rest of the transfer.", "Log it only if it repeats three times."),
            ScenarioQuestion("C2Q39", "ELC-C2", "C2-L4", "Which closeout detail most helps future crews?", "Useful closeout captures deviations and lessons learned.", "Clear notes on deviations and lessons learned.", 2, "Only transfer duration.", "Only cargo totals.", "Clear notes on deviations and lessons learned.", "Only weather summary."),
            ScenarioQuestion("C2Q40", "ELC-C2", "C2-L1", "Which statement best captures DFFE regulatory intent offshore?", "Control the operation so environmental harm is less likely and response is stronger.", "Control the operation to reduce harm and improve response.", 0, "Control the operation to reduce harm and improve response.", "Focus only on post-incident punishment.", "Shift all responsibility ashore.", "Limit documentation." )
        ];

        private static IReadOnlyList<Question> BuildCourse3Questions() =>
        [
            Mcq("C3Q01", "ELC-C3", "C3-L1", "Why are protected marine areas established?", "They protect vulnerable habitats, species, and ecological functions.", 1, "To speed up cargo operations", "To protect vulnerable habitats and species", "To reduce watchkeeping", "To replace route planning"),
            Mcq("C3Q02", "ELC-C3", "C3-L1", "What is the first operational question near a protected area?", "Spatial awareness is the first question.", 0, "Where are we relative to the protected boundary?", "Who finishes first?", "How fast can we transfer?", "Can we reduce recordkeeping?"),
            Mcq("C3Q03", "ELC-C3", "C3-L2", "What is an environmental receptor?", "A receptor is something in the environment that can be harmed.", 2, "A weather instrument", "A duty schedule", "A species, habitat, or ecological process at risk", "A communication device"),
            Mcq("C3Q04", "ELC-C3", "C3-L2", "Why does seasonal sensitivity matter?", "Breeding and migration patterns can change the level of risk.", 1, "It affects catering schedules", "It changes environmental vulnerability", "It removes permit conditions", "It shortens reporting duties"),
            Mcq("C3Q05", "ELC-C3", "C3-L3", "What is the practical purpose of a buffer zone?", "Buffers create separation and reaction margin.", 3, "To shorten routes", "To increase cargo rate", "To reduce training needs", "To create separation and reaction margin"),
            Mcq("C3Q06", "ELC-C3", "C3-L3", "Which factor can erode a buffer without a formal decision?", "Weather-driven drift can erode margin if not monitored.", 0, "Weather-driven drift", "A signed permit", "A completed meal break", "A closed checklist"),
            Mcq("C3Q07", "ELC-C3", "C3-L4", "What decision bias is appropriate when sensitivity increases unexpectedly?", "Bias toward caution protects sensitive areas.", 2, "Bias toward speed", "Bias toward silence", "Bias toward caution", "Bias toward reduced monitoring"),
            Mcq("C3Q08", "ELC-C3", "C3-L4", "What should happen if wildlife density increases near the operation?", "Environmental changes should be escalated and reassessed.", 1, "Ignore it until pollution occurs", "Escalate and reassess the operation", "Reduce reporting frequency", "Continue because wildlife often moves away"),
            Mcq("C3Q09", "ELC-C3", "C3-L1", "Why can impacts in protected areas be especially serious?", "Sensitive habitats can recover slowly and damage may be significant.", 0, "Recovery can be slow and consequences severe", "Because operations are always banned there", "Because wildlife never moves", "Because only large spills matter"),
            Mcq("C3Q10", "ELC-C3", "C3-L2", "Which observation may indicate heightened sensitivity?", "Wildlife aggregation is a meaningful operational cue.", 3, "Routine radio chatter", "Normal deck wash", "Steady cargo rate", "Wildlife aggregation near the operation"),
            Tf("C3Q11", "ELC-C3", "C3-L1", "Protected marine areas may contain habitats that recover slowly from disturbance.", true, "Sensitivity and slow recovery are common reasons for protected status."),
            Tf("C3Q12", "ELC-C3", "C3-L1", "Spatial awareness is less important than cargo timing near protected areas.", false, "Location and boundary control are critical near sensitive areas."),
            Tf("C3Q13", "ELC-C3", "C3-L2", "Seasonal migration can change environmental risk near the same location.", true, "Sensitivity is not always static."),
            Tf("C3Q14", "ELC-C3", "C3-L2", "Wildlife observations should only be recorded after pollution is confirmed.", false, "Wildlife observations can influence prevention decisions too."),
            Tf("C3Q15", "ELC-C3", "C3-L3", "Buffer zones should be actively monitored, not assumed.", true, "Buffers only work if crews verify that they are being maintained."),
            Tf("C3Q16", "ELC-C3", "C3-L3", "If the operation remains efficient, boundary drift is acceptable.", false, "Efficiency never overrides boundary control."),
            Tf("C3Q17", "ELC-C3", "C3-L4", "Unexpected wildlife density can justify reassessing an operation even before any pollution is visible.", true, "Sensitivity changes can require conservative control decisions."),
            Tf("C3Q18", "ELC-C3", "C3-L4", "Reduced visibility near a protected area may increase environmental risk.", true, "Reduced visibility can weaken monitoring and decision quality."),
            Tf("C3Q19", "ELC-C3", "C3-L1", "Protected area status is mainly symbolic and has little operational meaning.", false, "It has direct operational implications for planning and control."),
            Tf("C3Q20", "ELC-C3", "C3-L2", "Bird colonies, marine mammals, and reefs can all be relevant receptors.", true, "Multiple receptor types may need protection."),
            Tf("C3Q21", "ELC-C3", "C3-L3", "Support vessel movement can affect effective buffer margin.", true, "Operational movement can change spatial exposure."),
            Tf("C3Q22", "ELC-C3", "C3-L4", "A conservative pause is often better than a late reaction near a sensitive area.", true, "Early control protects both the environment and the operation."),
            Tf("C3Q23", "ELC-C3", "C3-L1", "Damage in sensitive areas can lead to ecological, regulatory, and reputational consequences.", true, "The consequences are often multi-dimensional."),
            Tf("C3Q24", "ELC-C3", "C3-L2", "Crew observation has no value in understanding receptor sensitivity offshore.", false, "Crew observation can be highly valuable when conditions change."),
            Tf("C3Q25", "ELC-C3", "C3-L3", "Weather and current changes can erode margins silently.", true, "Drift and changing conditions must be monitored continuously."),
            Tf("C3Q26", "ELC-C3", "C3-L3", "Buffer zones remove the need for active watchkeeping.", false, "Buffers complement monitoring; they do not replace it."),
            Tf("C3Q27", "ELC-C3", "C3-L4", "Environmental protection decisions near sensitive areas should favour caution when information is incomplete.", true, "Uncertainty is a reason for caution, not complacency."),
            Tf("C3Q28", "ELC-C3", "C3-L4", "If wildlife moves into the area, crews should never adjust operations unless product has leaked.", false, "Observed sensitivity changes can require operational reassessment."),
            ScenarioQuestion("C3Q29", "ELC-C3", "C3-L1", "The bridge team realises the planned transfer area is closer to a protected boundary than expected. What should they do?", "Boundary awareness must shape planning immediately.", "Pause planning and verify the safe operating position.", 2, "Proceed and monitor later.", "Reduce logging instead.", "Pause planning and verify the safe operating position.", "Ignore it if weather is calm."),
            ScenarioQuestion("C3Q30", "ELC-C3", "C3-L2", "Observers report unusual marine mammal activity near the operation. What is the best response?", "Unexpected receptor activity should trigger conservative reassessment.", "Escalate and reassess controls with caution.", 1, "Assume it is unrelated.", "Escalate and reassess controls with caution.", "Reduce watchkeeper reports.", "Wait until the operation is complete."),
            ScenarioQuestion("C3Q31", "ELC-C3", "C3-L3", "Weather and drift begin reducing buffer margin near a protected area. What should happen?", "Shrinking margin should trigger early corrective action.", "Pause or reposition before the boundary is compromised.", 0, "Pause or reposition before the boundary is compromised.", "Continue until the line is crossed.", "Only note it in the post-job report.", "Ignore it if product is not yet moving."),
            ScenarioQuestion("C3Q32", "ELC-C3", "C3-L4", "Bird density increases suddenly while the operation is setting up. What should the team do?", "Wildlife density can indicate heightened sensitivity.", "Bias toward caution and reassess whether to proceed.", 3, "Carry on because no spill exists.", "Reduce wildlife watch to avoid distraction.", "Call it normal and continue.", "Bias toward caution and reassess."),
            ScenarioQuestion("C3Q33", "ELC-C3", "C3-L2", "A watchkeeper is unsure whether a wildlife observation is significant. What should they do?", "Uncertainty should still be escalated when receptors may be sensitive.", "Report it and let the team assess the significance.", 1, "Ignore it unless a supervisor asks.", "Report it and let the team assess significance.", "Delete the note if uncertain.", "Only mention it after the transfer."),
            ScenarioQuestion("C3Q34", "ELC-C3", "C3-L1", "Why is a protected-area map not enough on its own?", "Maps must be supported by live situational awareness and control.", "Because active monitoring and boundary control are still required.", 2, "Because printed maps are always wrong.", "Because protected status changes every hour.", "Because active monitoring and control are still required.", "Because only regulators need maps."),
            ScenarioQuestion("C3Q35", "ELC-C3", "C3-L3", "A support craft drifts closer to a sensitive zone. What is the best response?", "Support vessel movement can change the real buffer.", "Reassess the boundary and correct the drift immediately.", 0, "Reassess the boundary and correct the drift.", "Ignore it because the main vessel is stable.", "Wait for the next watch.", "Stop recording positions."),
            ScenarioQuestion("C3Q36", "ELC-C3", "C3-L4", "Visibility deteriorates near a sensitive area. What should happen?", "Reduced monitoring quality should influence the go/no-go decision.", "Increase caution and reassess whether controls remain adequate.", 1, "Continue because visibility is only a navigation issue.", "Increase caution and reassess controls.", "Reduce communication traffic.", "Skip wildlife watch to focus elsewhere."),
            ScenarioQuestion("C3Q37", "ELC-C3", "C3-L1", "What is the strongest reason to stay outside protected buffers?", "Distance reduces both probability and severity of impact.", "It preserves margin and reduces ecological risk.", 2, "It speeds up reporting.", "It reduces maintenance cost.", "It preserves margin and ecological safety.", "It eliminates all legal duties."),
            ScenarioQuestion("C3Q38", "ELC-C3", "C3-L2", "A crew member suggests wildlife observations are irrelevant if the sea is calm. What should be clarified?", "Receptor sensitivity matters regardless of sea state.", "Wildlife observations can still change risk decisions.", 3, "Calm sea means environmental sensitivity is low.", "Only pollution matters.", "Wildlife never affects permit limits.", "Wildlife observations can still change risk decisions."),
            ScenarioQuestion("C3Q39", "ELC-C3", "C3-L3", "Which action best protects a buffer zone?", "Active tracking and early intervention protect buffers.", "Monitoring position trends and acting before limits are reached.", 0, "Monitoring position trends and acting early.", "Relying on a start-of-shift estimate.", "Watching only the cargo system.", "Delaying action until the boundary is crossed."),
            ScenarioQuestion("C3Q40", "ELC-C3", "C3-L4", "What best defines conservative decision-making near a protected area?", "Choosing the option that best preserves environmental margin under uncertainty.", "Choosing the option that preserves margin under uncertainty.", 1, "Choosing the fastest option.", "Choosing the option that preserves margin under uncertainty.", "Choosing the least documented option.", "Choosing the cheapest option." )
        ];

        private static IReadOnlyList<Question> BuildCourse4Questions() =>
        [
            Mcq("C4Q01", "ELC-C4", "C4-L1", "How should environmental responsibility be understood on board?", "It is a shared responsibility supported by clear task ownership.", 0, "As a shared responsibility", "As an officer-only duty", "As a shore-only concern", "As a post-incident activity"),
            Mcq("C4Q02", "ELC-C4", "C4-L1", "What should a crew member do if they see an environmental control being bypassed?", "They should challenge and escalate it through the proper process.", 2, "Ignore it", "Wait for audit season", "Challenge and escalate it", "Rewrite the permit"),
            Mcq("C4Q03", "ELC-C4", "C4-L2", "Why is housekeeping part of environmental compliance?", "Poor housekeeping can create accidental release pathways.", 1, "It only improves appearance", "It reduces accidental release pathways", "It replaces monitoring", "It removes the need for watchkeeping"),
            Mcq("C4Q04", "ELC-C4", "C4-L2", "How should minor leaks be treated?", "Minor leaks are warnings that deserve action.", 3, "As acceptable offshore conditions", "As non-reportable defects", "As cosmetic only", "As warning signals requiring action"),
            Mcq("C4Q05", "ELC-C4", "C4-L3", "What is the value of effective watchkeeping?", "It enables early detection and early control action.", 2, "It removes the need for procedures", "It shortens training time", "It supports early detection and control", "It replaces leadership"),
            Mcq("C4Q06", "ELC-C4", "C4-L3", "What is a correct immediate response principle when safe to do so?", "Stop the source, alert command, and contain early.", 0, "Stop the source, alert command, and contain early", "Wait for shore before acting", "Delete logs to reduce confusion", "Avoid documenting until cleanup ends"),
            Mcq("C4Q07", "ELC-C4", "C4-L4", "What best supports compliance culture?", "Early reporting and constructive challenge.", 1, "Silence about weak signals", "Early reporting and constructive challenge", "Punishing all errors publicly", "Avoiding near-miss records"),
            Mcq("C4Q08", "ELC-C4", "C4-L4", "Why are near misses valuable?", "They help strengthen controls before future events occur.", 2, "They only matter to insurers", "They reduce paperwork", "They help strengthen controls", "They replace investigations"),
            Mcq("C4Q09", "ELC-C4", "C4-L2", "Why should containers and stores be secured carefully offshore?", "Sea state can turn poor storage into pollution quickly.", 3, "To reduce navigation time", "To improve comfort", "To reduce radio traffic", "Sea state can turn poor storage into pollution quickly"),
            Mcq("C4Q10", "ELC-C4", "C4-L1", "What is the best professional standard for crew environmental performance?", "Vigilance, discipline, and speaking up early.", 0, "Vigilance, discipline, and speaking up early", "Leaving compliance to specialists", "Prioritising schedule over risk", "Avoiding written reports"),
            Tf("C4Q11", "ELC-C4", "C4-L1", "Clear role allocation improves environmental response quality.", true, "Role clarity supports response speed and coordination."),
            Tf("C4Q12", "ELC-C4", "C4-L1", "Shared responsibility means no one needs specific duties.", false, "Shared responsibility still requires clear role allocation."),
            Tf("C4Q13", "ELC-C4", "C4-L2", "Housekeeping failures can become environmental release pathways.", true, "Poor housekeeping often weakens barriers."),
            Tf("C4Q14", "ELC-C4", "C4-L2", "Waste segregation can be ignored offshore if the transfer is time-critical.", false, "Waste controls remain important regardless of schedule pressure."),
            Tf("C4Q15", "ELC-C4", "C4-L3", "The first minutes of an environmental incident are often critical.", true, "Early action can reduce the eventual consequence substantially."),
            Tf("C4Q16", "ELC-C4", "C4-L3", "Evidence collection should always wait until the next shift arrives.", false, "Early evidence capture is important if safe."),
            Tf("C4Q17", "ELC-C4", "C4-L4", "A blame-heavy culture can reduce the quality of environmental reporting.", true, "Fear often suppresses timely escalation."),
            Tf("C4Q18", "ELC-C4", "C4-L4", "Near misses should be ignored if no pollution reached the sea.", false, "Near misses are valuable indicators of system weakness."),
            Tf("C4Q19", "ELC-C4", "C4-L2", "Heavy weather can worsen the consequences of small equipment defects.", true, "Environmental exposure can rise quickly when conditions deteriorate."),
            Tf("C4Q20", "ELC-C4", "C4-L1", "Only the designated environmental watchkeeper should report pollution concerns.", false, "Anyone observing a concern should report it."),
            Tf("C4Q21", "ELC-C4", "C4-L3", "Immediate communication is part of effective environmental incident response.", true, "Fast communication supports coordination and containment."),
            Tf("C4Q22", "ELC-C4", "C4-L2", "Secure storage and drip control are environmental controls as well as housekeeping measures.", true, "They directly reduce release probability."),
            Tf("C4Q23", "ELC-C4", "C4-L4", "Leaders who welcome challenge usually strengthen compliance culture.", true, "Psychological safety supports better reporting and escalation."),
            Tf("C4Q24", "ELC-C4", "C4-L3", "Stopping the source when safe is a common early response principle.", true, "Source control limits escalation when performed safely."),
            Tf("C4Q25", "ELC-C4", "C4-L1", "Shared environmental responsibility means silence is acceptable if someone senior is present.", false, "Presence of seniors does not remove the duty to speak up."),
            Tf("C4Q26", "ELC-C4", "C4-L2", "Absorbent pads alone are an adequate long-term response to a persistent leak.", false, "The source must be investigated and corrected."),
            Tf("C4Q27", "ELC-C4", "C4-L3", "Chronological records help explain what happened during an incident.", true, "Chronology strengthens investigation and defensibility."),
            Tf("C4Q28", "ELC-C4", "C4-L4", "Learning from deviations is part of strong environmental performance.", true, "Continuous learning is a hallmark of resilient operations."),
            ScenarioQuestion("C4Q29", "ELC-C4", "C4-L1", "A rating notices a control step is being skipped but assumes it is not their place to speak. What is best?", "Rank does not remove the duty to escalate risk.", "Raise the concern through the proper chain immediately.", 1, "Keep silent and let supervisors manage it.", "Raise the concern through the proper chain immediately.", "Only mention it after the shift.", "Wait for an incident."),
            ScenarioQuestion("C4Q30", "ELC-C4", "C4-L2", "A small leak is found near transfer equipment before weather worsens. What should the crew do?", "Minor leaks are warning signals requiring immediate action.", "Contain, trace the source, and escalate.", 0, "Contain, trace the source, and escalate.", "Ignore it because the deck is already busy.", "Cover it and continue.", "Wait for a post-job repair."),
            ScenarioQuestion("C4Q31", "ELC-C4", "C4-L3", "A watchkeeper sees a sheen and an alarm at the same time. What is the best first response?", "Immediate coordinated action matters.", "Alert command, control the source if safe, and start evidence capture.", 2, "Wait until both are confirmed by shore.", "Reset the alarm and continue.", "Alert command, control the source if safe, and preserve evidence.", "Avoid reporting until the spill is measured."),
            ScenarioQuestion("C4Q32", "ELC-C4", "C4-L4", "A supervisor discourages near-miss reporting to keep the operation looking clean. What should happen?", "Hiding weak signals weakens compliance culture.", "Record the near miss and escalate the concern if needed.", 3, "Accept the supervisor’s view.", "Delete draft notes.", "Wait for annual review.", "Record the near miss and escalate if needed."),
            ScenarioQuestion("C4Q33", "ELC-C4", "C4-L2", "Waste containers are unsecured on a deck exposed to bad weather. What is the best response?", "Poor storage is a pollution risk.", "Secure them immediately and record the deficiency.", 1, "Leave them if lids are closed.", "Secure them immediately and record the deficiency.", "Move them after the operation ends.", "Ignore them if no waste has escaped."),
            ScenarioQuestion("C4Q34", "ELC-C4", "C4-L3", "During response, one crew member says documentation can wait. What should the team remember?", "Response and documentation should run in parallel where safe.", "Capture key facts and timings while stabilising the situation.", 0, "Capture key facts and timings while stabilising the situation.", "Documentation is never necessary.", "Only managers may record facts.", "Wait until memory is less fresh."),
            ScenarioQuestion("C4Q35", "ELC-C4", "C4-L1", "A crew member reports a concern that turns out to be harmless. What cultural response is best?", "Early reporting should be reinforced, not punished.", "Thank them and reinforce the value of speaking up.", 2, "Criticise them for wasting time.", "Tell them to keep quiet next time.", "Thank them and reinforce speaking up.", "Remove them from watchkeeping."),
            ScenarioQuestion("C4Q36", "ELC-C4", "C4-L4", "Which action best strengthens environmental culture after a near miss?", "A structured review and lesson-sharing strengthens culture.", "Review the event and update training or procedures.", 3, "Keep it within one team only.", "Avoid documenting it.", "Treat it as bad luck only.", "Review it and update training or procedures."),
            ScenarioQuestion("C4Q37", "ELC-C4", "C4-L2", "Absorbents are saturated after controlling a drip. What next?", "Waste and source control both matter.", "Replace them, manage the waste correctly, and investigate the source.", 1, "Leave them until the shift ends.", "Replace them, manage waste correctly, and investigate the source.", "Wash them overboard.", "Ignore them if the deck looks clean."),
            ScenarioQuestion("C4Q38", "ELC-C4", "C4-L3", "What is the best first-response balance during an incident?", "Protect people, contain the source if safe, communicate, and document.", "Protect people, contain if safe, communicate, and document.", 0, "Protect people, contain if safe, communicate, and document.", "Contain only and avoid communications.", "Document only after the fact.", "Wait for shore instructions before any local action."),
            ScenarioQuestion("C4Q39", "ELC-C4", "C4-L4", "A leader reacts angrily to reported weak signals. What risk does that create?", "It can suppress future reporting and weaken compliance.", "It discourages future escalation and reporting.", 2, "It makes reporting more accurate.", "It improves morale immediately.", "It discourages future escalation and reporting.", "It removes the need for audits."),
            ScenarioQuestion("C4Q40", "ELC-C4", "C4-L1", "What best defines professional environmental seamanship?", "Disciplined execution, vigilance, challenge, and accurate reporting.", "Disciplined execution, vigilance, challenge, and accurate reporting.", 0, "Disciplined execution, vigilance, challenge, and accurate reporting.", "Fast operations with minimal interruption.", "Leaving compliance to specialists.", "Avoiding written evidence." )
        ];

        private static Slide Slide(string title, string? bodyHtml, params string[] bullets) => new()
        {
            Title = title,
            BodyHtml = bodyHtml,
            Bullets = bullets
        };

        private static Flashcard Flashcard(string category, string front, string back) => new()
        {
            Category = category,
            FrontText = front,
            BackText = back
        };

        private static Question Mcq(string id, string courseCode, string? lessonCode, string prompt, string explanation, int correctIndex, params string[] options) => new()
        {
            QuestionId = id,
            CourseCode = courseCode,
            LessonCode = lessonCode,
            Kind = QuestionKind.MultipleChoice,
            Prompt = prompt,
            Explanation = explanation,
            Options = options,
            CorrectOptionIndexes = new HashSet<int> { correctIndex }
        };

        private static Question Tf(string id, string courseCode, string? lessonCode, string prompt, bool isTrue, string explanation) => new()
        {
            QuestionId = id,
            CourseCode = courseCode,
            LessonCode = lessonCode,
            Kind = QuestionKind.TrueFalse,
            Prompt = prompt,
            Explanation = explanation,
            Options = ["True", "False"],
            CorrectOptionIndexes = new HashSet<int> { isTrue ? 0 : 1 }
        };

        private static Question ScenarioQuestion(string id, string courseCode, string? lessonCode, string prompt, string scenarioContext, string explanation, int correctIndex, params string[] options) => new()
        {
            QuestionId = id,
            CourseCode = courseCode,
            LessonCode = lessonCode,
            Kind = QuestionKind.Scenario,
            Prompt = prompt,
            ScenarioContext = scenarioContext,
            Explanation = explanation,
            Options = options,
            CorrectOptionIndexes = new HashSet<int> { correctIndex }
        };

        private static ScenarioStep Step(string id, string title, string prompt, string? videoUrl, string? guidance, params ScenarioChoice[] choices) => new()
        {
            StepId = id,
            Title = title,
            Prompt = prompt,
            VideoUrl = videoUrl,
            Guidance = guidance,
            Choices = choices
        };

        private static ScenarioChoice Choice(string id, string text, string? nextStepId, bool isPreferred, decimal score, string feedback) => new()
        {
            ChoiceId = id,
            Text = text,
            NextStepId = nextStepId,
            IsPreferred = isPreferred,
            Score = score,
            Feedback = feedback
        };
    }
}
