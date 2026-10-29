import os
from xml.sax.saxutils import escape
from PIL import Image, ImageDraw, ImageFont
from docx import Document
from docx.shared import Inches, Pt
from docx.enum.text import WD_ALIGN_PARAGRAPH

ROOT = os.path.dirname(os.path.abspath(__file__))
FONT = r"C:\Windows\Fonts\arial.ttf"
FONT_B = r"C:\Windows\Fonts\arialbd.ttf"

COLORS = {
    "person": ("#08427B", "#FFFFFF"),
    "system": ("#1168BD", "#FFFFFF"),
    "external": ("#999999", "#FFFFFF"),
    "container": ("#438DD5", "#FFFFFF"),
    "db": ("#2E7D32", "#FFFFFF"),
    "queue": ("#E65100", "#FFFFFF"),
}


def border_point(n, tx, ty):
    cx, cy = n["x"] + n["w"] / 2, n["y"] + n["h"] / 2
    dx, dy = tx - cx, ty - cy
    if dx == 0 and dy == 0:
        return cx, cy
    sx = (n["w"] / 2) / abs(dx) if dx else float("inf")
    sy = (n["h"] / 2) / abs(dy) if dy else float("inf")
    s = min(sx, sy)
    return cx + dx * s, cy + dy * s


def render_png(spec, path):
    w, h = spec["size"]
    img = Image.new("RGB", (w, h), "white")
    d = ImageDraw.Draw(img)
    f = ImageFont.truetype(FONT, 13)
    fb = ImageFont.truetype(FONT_B, 14)
    ft = ImageFont.truetype(FONT_B, 20)
    d.text((20, 12), spec["title"], fill="#222222", font=ft)
    nodes = {n["id"]: n for n in spec["nodes"]}
    for e in spec["edges"]:
        a, b = nodes[e["from"]], nodes[e["to"]]
        acx, acy = a["x"] + a["w"] / 2, a["y"] + a["h"] / 2
        bcx, bcy = b["x"] + b["w"] / 2, b["y"] + b["h"] / 2
        p1 = border_point(a, bcx, bcy)
        p2 = border_point(b, acx, acy)
        d.line([p1, p2], fill="#444444", width=2)
        import math
        ang = math.atan2(p2[1] - p1[1], p2[0] - p1[0])
        for da in (2.7, -2.7):
            d.line([p2, (p2[0] + 11 * math.cos(ang + da), p2[1] + 11 * math.sin(ang + da))], fill="#444444", width=2)
        mx, my = (p1[0] + p2[0]) / 2, (p1[1] + p2[1]) / 2
        label = e["label"]
        tw = d.textlength(label, font=f)
        d.rectangle([mx - tw / 2 - 3, my - 9, mx + tw / 2 + 3, my + 9], fill="white")
        d.text((mx - tw / 2, my - 8), label, fill="#222222", font=f)
    for n in spec["nodes"]:
        bg, fg = COLORS[n["kind"]]
        d.rounded_rectangle([n["x"], n["y"], n["x"] + n["w"], n["y"] + n["h"]], radius=8, fill=bg, outline="#222222")
        lines = [(n["name"], fb)] + [(t, f) for t in n["desc"].split("\n")]
        total = sum(18 for _ in lines)
        y = n["y"] + (n["h"] - total) / 2
        for t, fnt in lines:
            tw = d.textlength(t, font=fnt)
            d.text((n["x"] + (n["w"] - tw) / 2, y), t, fill=fg, font=fnt)
            y += 18
    img.save(path)


def render_drawio(spec, path):
    cells = ['<mxCell id="0"/>', '<mxCell id="1" parent="0"/>']
    for n in spec["nodes"]:
        bg, fg = COLORS[n["kind"]]
        shape = "shape=cylinder3;" if n["kind"] == "db" else ""
        style = f"rounded=1;{shape}whiteSpace=wrap;html=1;fillColor={bg};fontColor={fg};strokeColor=#222222;"
        label = f"<b>{escape(n['name'])}</b><br>" + "<br>".join(escape(t) for t in n["desc"].split("\n"))
        cells.append(
            f'<mxCell id="{n["id"]}" value="{escape(label, {chr(34): "&quot;"})}" style="{style}" vertex="1" parent="1">'
            f'<mxGeometry x="{n["x"]}" y="{n["y"]}" width="{n["w"]}" height="{n["h"]}" as="geometry"/></mxCell>')
    for i, e in enumerate(spec["edges"]):
        cells.append(
            f'<mxCell id="e{i}" value="{escape(e["label"])}" style="edgeStyle=orthogonalEdgeStyle;rounded=1;html=1;endArrow=block;" '
            f'edge="1" parent="1" source="{e["from"]}" target="{e["to"]}"><mxGeometry relative="1" as="geometry"/></mxCell>')
    xml = (f'<mxfile host="app.diagrams.net"><diagram name="{escape(spec["title"])}" id="{spec["file"]}">'
           f'<mxGraphModel dx="1200" dy="800" grid="1" gridSize="10" page="1" pageWidth="{spec["size"][0]}" pageHeight="{spec["size"][1]}">'
           f'<root>{"".join(cells)}</root></mxGraphModel></diagram></mxfile>')
    with open(path, "w", encoding="utf-8") as fh:
        fh.write(xml)


def N(id, kind, name, desc, x, y, w=190, h=90):
    return dict(id=id, kind=kind, name=name, desc=desc, x=x, y=y, w=w, h=h)


def E(a, b, label):
    return {"from": a, "to": b, "label": label}


DIAGRAMS = [
    dict(file="architecture-c4-1-context", title="C4 Level 1 - System Context", size=(1000, 560),
         nodes=[
             N("cust", "person", "Customer", "Browses boards,\ncreates/edits/cancels own orders", 40, 80),
             N("mgr", "person", "Manager", "Manages boards, sees all orders,\nprocesses/completes/cancels", 40, 330),
             N("sys", "system", "MountyPython", "SMT order management\n(boards, components, orders)", 400, 200, 220, 110),
             N("kc", "external", "Keycloak", "Identity provider\n(OIDC / JWT, realm mountypython)", 760, 80),
             N("line", "external", "Production line", "Receives downloaded order\n(simulated)", 760, 330),
         ],
         edges=[E("cust", "sys", "Uses (HTTPS)"), E("mgr", "sys", "Uses (HTTPS)"),
                E("sys", "kc", "Authenticates users"), E("sys", "line", "Order download (JSON)")]),
    dict(file="architecture-c4-2-container", title="C4 Level 2 - Containers", size=(1300, 760),
         nodes=[
             N("spa", "container", "Angular Client", "NgModule, NgRx, Material,\nSignal Forms", 40, 300),
             N("yarp", "container", "YARP frontend-proxy", "Static hosting + API/Keycloak\nrouting (publish mode)", 300, 300),
             N("kc", "external", "Keycloak 26.8", "OIDC, realm import,\ncustom theme", 300, 560),
             N("inv", "container", "Inventory Services", "Boards & components\nMinimal API, EF Core", 620, 60),
             N("ops", "container", "Operative Services", "Orders, lifecycle, download\nMinimal API, EF Core", 620, 300),
             N("ntf", "container", "Notification Services", "SignalR hub /api/v1/updateshub\nRebus consumers", 620, 560),
             N("mq", "queue", "RabbitMQ", "Rebus transport\n(topic exchanges)", 980, 300),
             N("pgi", "db", "PostgreSQL: inventory", "mountypython-inventorydb", 980, 60, 190, 100),
             N("pgo", "db", "PostgreSQL: operative", "mountypython-operativedb", 980, 440, 190, 100),
         ],
         edges=[E("spa", "yarp", "HTTPS"), E("yarp", "inv", "/api/catalogs"), E("yarp", "ops", "/api/operations"),
                E("yarp", "ntf", "SignalR / WebSocket"), E("yarp", "kc", "/realms"),
                E("inv", "pgi", "EF Core"), E("ops", "pgo", "EF Core"),
                E("inv", "mq", "Publish Board*"), E("ops", "mq", "Publish Order*"),
                E("mq", "ntf", "Deliver events"), E("mq", "ops", "BoardUpdated/Deleted")]),
    dict(file="architecture-event-flow", title="Runtime - Event-driven notification flow", size=(1200, 460),
         nodes=[
             N("cmd", "container", "Command", "e.g. CreateOrderCommand /\nUpdateBoardCommand", 30, 160, 190, 100),
             N("repo", "container", "Repository", "SaveChangesAsync, then\nIPublisher publishes domain events", 270, 160, 220, 100),
             N("mq", "queue", "RabbitMQ", "Rebus topic exchange\nper event type", 540, 160, 190, 100),
             N("cons", "container", "Consumers", "IHandleMessages<TEvent>\n(Notification, Operative)", 780, 160, 190, 100),
             N("hub", "container", "SignalR Hub", "Push to connected clients", 1010, 160, 160, 100),
         ],
         edges=[E("cmd", "repo", "Domain event"), E("repo", "mq", "Publish"), E("mq", "cons", "Subscribe<T>()"),
                E("cons", "hub", "Push update")]),
]


SECTIONS = [
    ("1. Introduction and Goals", [
        ("p", "MountyPython manages orders in an SMT (Surface-Mount Technology) manufacturing environment. It supports creating, editing, searching and deleting Orders, Boards and Components, and downloading an order to simulate a hand-off to a production line. Motto: Always look on the bright side of the board."),
        ("h2", "1.1 Requirements overview"),
        ("b", ["An Order contains one or more Boards (with quantity); a Board consists of one or more Components.",
               "Order: Name, Description, Order Date. Board: Name, Description, Length, Width. Component: Name, Description, Quantity.",
               "Customers see available boards, create/edit/cancel and see only their own orders.",
               "Managers edit/delete boards, see all orders and move an order to Processing, Completed or Cancelled.",
               "An order may be changed or cancelled only while its status is Created; items (boards, quantities) are editable in that state only.",
               "Order download as JSON; web UI behind login; persistence across restarts; logging; unit tests; CI/CD; containerization."]),
        ("h2", "1.2 Quality goals"),
        ("t", ["Priority", "Goal", "Motivation"], [
            ["1", "Time-to-market / low cost", "More than a prototype, low development, maintenance and running cost, no paid licenses."],
            ["2", "Maintainability", "Vertical slices, DDD, CQRS-style commands/queries, tests (xUnit v3 + AwesomeAssertions)."],
            ["3", "Security", "Keycloak (OIDC/JWT), role-based access (customer vs manager), ownership guard on orders."],
            ["4", "Resilience & observability", "Retries, fail-fast, timeouts, health checks, OpenTelemetry, Serilog."],
            ["5", "Usability", "Mobile-first Angular Material UI."]]),
        ("h2", "1.3 Stakeholders"),
        ("t", ["Role", "Expectation"], [
            ["Customer", "Simple ordering of boards, sees only own orders, real-time status updates."],
            ["Manager", "Maintains catalog, controls order lifecycle, overview of all orders."],
            ["Development team", "Clear structure, fast feedback through CI, easy local start via Aspire."],
            ["Operations", "Reproducible provisioning on Azure, monitoring."]]),
    ]),
    ("2. Architecture Constraints", [
        ("t", ["Constraint", "Background"], [
            ["Technology: .NET 10 / C# 14, ASP.NET Core Minimal APIs, EF Core, PostgreSQL", "Chosen stack, cross-platform."],
            ["Orchestration: .NET Aspire (AppHost)", "Single composition root for infrastructure and service discovery."],
            ["Identity: Keycloak", "Free identity provider instead of Azure AD B2C."],
            ["Messaging: RabbitMQ with Rebus", "Rebus instead of MassTransit (licensing)."],
            ["No MediatR / AutoMapper / FluentAssertions", "License costs; hand-written commands, consumers and DTO mapping; AwesomeAssertions used."],
            ["Frontend: Angular (NgModule based), Angular Material, NgRx, Signal Forms, Node 24", "Mobile-first, state management via NgRx."],
            ["Hosting: Azure Container Apps, GitHub, GitHub Actions", "Free-tier friendly cloud deployment."],
            ["Domain model changes require approval", "Team convention (copilot-instructions)."]]),
    ]),
    ("3. Context and Scope", [
        ("h2", "3.1 Business context"),
        ("img", "architecture-c4-1-context"),
        ("t", ["Partner", "Input", "Output"], [
            ["Customer", "Create/edit/cancel orders, search boards", "Order status, real-time notifications"],
            ["Manager", "Maintain boards, change order status", "All orders, notifications"],
            ["Keycloak", "Credentials", "JWT access tokens (audience mountypython-api)"],
            ["Production line (simulated)", "-", "Downloaded order file (JSON)"]]),
        ("h2", "3.2 Technical context"),
        ("b", ["Browser to Angular app over HTTPS; locally on port 4200 with dev proxy (src/proxy.conf.js), in Azure via YARP.",
               "API paths: /api/catalogs (Inventory), /api/operations (Operative), notification and SignalR paths, plus Keycloak paths.",
               "Services to PostgreSQL through Npgsql; services to RabbitMQ through Rebus; all services validate JWTs issued by Keycloak."]),
    ]),
    ("4. Solution Strategy", [
        ("t", ["Goal", "Approach"], [
            ["Fast delivery, low complexity", "Aspire provisions all resources; shared Commons library for logging, repository, messaging, security."],
            ["Independent evolution", "3 microservices (Inventory, Operative, Notification) with own databases (Inventory, Operative)."],
            ["Loose coupling", "Event-driven architecture: domain events published through Rebus/RabbitMQ; consumers subscribe per event type."],
            ["Maintainability", "Onion/Clean architecture with vertical slices; Commands/Queries/Consumers per file with their interface; DDD domain model with validators."],
            ["Security", "Keycloak realm import (import.json), JWT bearer, manager/customer authorization, ownership guard."],
            ["Resilience", "ServiceDefaults: service discovery, resilient HttpClient (retry, fail-fast, timeout), health checks."],
            ["Quality", "TDD, xUnit v3 on Microsoft Testing Platform, CI on every push; Playwright E2E planned."],
            ["Operations", "GitHub Actions: dev-branch deploys to development, main to production; automatic versioning and tagging."]]),
    ]),
    ("5. Building Block View", [
        ("h2", "5.1 Level 1 - Containers (C4)"),
        ("img", "architecture-c4-2-container"),
        ("t", ["Building block", "Responsibility"], [
            ["MountyPython.AppHost", "Composition root: PostgreSQL, RabbitMQ, Keycloak, three services, Angular app, YARP (publish)."],
            ["MountyPython.ServiceDefaults", "OpenTelemetry, service discovery, resilient HttpClient, /health and /alive (development)."],
            ["Inventory.Services", "Catalog of Boards and Components: CRUD, search (case and accent insensitive), publishes BoardCreated/Updated/Deleted."],
            ["Operative.Services", "Orders: create, update (name/description and items while Created), start processing, complete, cancel, download; consumes board events."],
            ["Notification.Services", "Consumes order and board events and pushes updates to clients through the SignalR hub."],
            ["Commons (Backend/Shared)", "Domain base types, repository, publisher, logging, JWT/authorization, exception handling, JSON defaults."],
            ["Angular client", "Feature modules mirroring backend slices (inventory, operations); NgRx stores for boards, orders and auth."]]),
        ("h2", "5.2 Level 2 - Service internals"),
        ("b", ["Features/<area>: Commands, Queries, Consumers, Domain and a Map...Endpoints module that creates tagged route groups.",
               "Queries return aggregate DTOs; commands return domain entities which the endpoint maps to DTOs.",
               "Program.cs owns startup wiring (auth, validation, Rebus, DbContext, migrations, OpenAPI/Scalar in development).",
               "Each service owns its DbContext and migrations; MigratorHostedService<TContext> applies migrations on startup."]),
    ]),
    ("6. Runtime View", [
        ("h2", "6.1 Create order"),
        ("b", ["Customer submits name, date and items (board snapshot with components, quantity) in the Angular form.",
               "NgRx effect calls POST /api/operations/orders; CreateOrderCommand builds board/component snapshots and the Order aggregate.",
               "Repository saves and publishes OrderCreatedEvent; Notification pushes the update to connected clients."]),
        ("h2", "6.2 Edit order (status Created)"),
        ("b", ["Details/edit open an NgRx action that loads the full order by ID; the list only holds aggregates.",
               "The form allows adding/removing boards and changing quantities; PUT sends name, description and items.",
               "Order.UpdateItems rejects the change unless status is Created."]),
        ("h2", "6.3 Board update propagates"),
        ("img", "architecture-event-flow"),
        ("b", ["Manager updates a board; components are matched by PK and updated in place.",
               "BoardUpdatedEvent is published; Operative and Notification consume it through their Rebus subscriptions."]),
        ("h2", "6.4 Login"),
        ("b", ["Angular redirects to Keycloak (OIDC); the token is kept in session storage so refresh keeps the session; API calls carry the bearer token."]),
    ]),
    ("7. Deployment View", [
        ("t", ["Environment", "Description"], [
            ["Local", "Aspire runs PostgreSQL (with pgAdmin), RabbitMQ (management), Keycloak (port 9001, realm import, theme bind-mount) as containers; services as processes; Angular dev server on 4200. Docker required."],
            ["Development (Azure)", "Deployed from dev-branch to an Azure Container Apps environment (mountypython-dev), Azure PostgreSQL Flexible Server, YARP frontend-proxy serving the built client."],
            ["Production (Azure)", "Promoted from main (tagged, verified commit) to mountypython-prod; same topology."]]),
        ("b", ["Provisioning is generated by Aspire (no manual steps); publish-mode resource and route configuration must be preserved.",
               "Keycloak sits behind ACA HTTPS ingress; YARP builds https URLs to it."]),
    ]),
    ("8. Cross-cutting Concepts", [
        ("t", ["Concept", "Realization"], [
            ["Domain model", "DDD aggregates (Order, Board) with value objects (OrderItem, Component), FluentValidation at API and domain level, domain events."],
            ["Persistence", "EF Core + Npgsql via SetupNpgsqlServerDbContext; PostgreSQL unaccent for accent/case insensitive search."],
            ["Security", "JWT bearer, manager/customer authorization, OrderOwnershipGuard; Scalar/OpenAPI development only."],
            ["Messaging", "Rebus handlers auto-registered per assembly; events require explicit Subscribe<T>() per consuming service."],
            ["Logging & monitoring", "Serilog, OpenTelemetry via ServiceDefaults, Azure Application Insights."],
            ["Error handling", "Custom exception handler middleware mapping domain/validation/not-found errors."],
            ["Testing", "xUnit v3 + AwesomeAssertions on Microsoft Testing Platform; Angular tests with Vitest."],
            ["Frontend", "Mobile-first Material design, NgRx state, Signal Forms, styling in SCSS, global app-dialog class."],
            ["Mapping", "Hand-written DTO mapping instead of AutoMapper."]]),
    ]),
    ("9. Architecture Decisions", [
        ("t", ["ID", "Decision", "Rationale"], [
            ["ADR-1", "Aspire for orchestration and provisioning", "Single model for local and Azure; fewer manual steps."],
            ["ADR-2", "3 microservices instead of a monolith", "Aspire lowers cost; separate notification and business concerns."],
            ["ADR-3", "Rebus instead of MassTransit", "License cost."],
            ["ADR-4", "Commands and Consumers instead of MediatR", "License cost, simplicity."],
            ["ADR-5", "Keycloak instead of Azure AD B2C", "Free, self-hosted, realm as code."],
            ["ADR-6", "YARP serves the Angular static app", "BFF-style single entry point in Azure."],
            ["ADR-7", "Orders hold board snapshots", "Orders remain stable if the catalog changes; items editable only while Created."],
            ["ADR-8", "Database per service, migrations on startup", "Autonomy; zero-touch deployment."]]),
    ]),
    ("10. Quality Requirements", [
        ("t", ["Quality", "Scenario", "Measure"], [
            ["Security", "Customer requests another customer's order", "404 Not Found (ownership guard)"],
            ["Consistency", "Order changed after processing started", "Rejected by domain validation"],
            ["Resilience", "Downstream service slow or failing", "Retry, fail-fast, and timeout from ServiceDefaults"],
            ["Observability", "Failure in production", "Traces, metrics and logs in OpenTelemetry / Application Insights"],
            ["Usability", "Edit dialog on phone", "Single-column mobile-first layout"]]),
    ]),
    ("11. Risks and Technical Debt", [
        ("b", ["Missing Rebus Subscribe<T>() calls silently stop consumers from running; add integration tests covering delivery.",
               "Board snapshots in orders can diverge from the catalog (intentional, but must be communicated).",
               "UpdateItems reuses OrderUpdatedNameOrDescriptionEvent; a dedicated event may be needed.",
               "Idempotent requests are planned but not fully implemented; horizontal scalability and E2E (Playwright) tests are 'if time allows'.",
               "Angular tests executed through Visual Studio Test Explorer fail on JIT configuration; use npx ng test.",
               "Circuit breaker covers HTTP only: AddStandardResilienceHandler (ServiceDefaults) protects HttpClient calls, but PostgreSQL and RabbitMQ/Rebus are not covered, and the services currently do not call each other over HTTP, so it protects little today.",
               "Circuit breaker defaults do not fit the traffic: it opens only after at least 100 requests in a 10s window (10% failure ratio, 5s break); tune MinimumThroughput (5-10), FailureRatio (about 0.5), SamplingDuration (about 30s) and BreakDuration (15-30s) per client.",
               "No observability for resilience: log and alert on circuit OnOpened/OnHalfOpened/OnClosed, expose an open-circuit metric and a degraded health check.",
               "Messaging resilience is implicit: configure Rebus retries (max delivery attempts), the error queue and alerting on its depth explicitly.",
               "Events can be lost on a broker outage: Publisher publishes right after SaveChangesAsync; a transactional outbox is the proper fix.",
               "No fallback behaviour defined: notifications down must not fail orders (fire-and-forget), Keycloak down should fail fast with 503, and the UI should show a retry message.",
               "No tests for resilience: add unit tests with a failing HttpMessageHandler asserting the circuit opens, and optionally a chaos test stopping RabbitMQ locally."]),
    ]),
    ("12. Glossary", [
        ("t", ["Term", "Meaning"], [
            ["SMT", "Surface-Mount Technology, PCB assembly process."],
            ["Board", "Printed circuit board design with its components."],
            ["Component", "Part placed on a board with a quantity."],
            ["Order", "Request to produce boards in given quantities; status Created, Processing, Completed or Cancelled."],
            ["Aspire", ".NET application host for orchestration and provisioning."],
            ["Rebus", "Service bus library used over RabbitMQ."],
            ["YARP", "Yet Another Reverse Proxy."],
            ["C4 / arc42", "Documentation models used for the diagrams and the structure of this document."]]),
    ]),
]


def build_doc():
    doc = Document()
    st = doc.styles["Normal"]
    st.font.name = "Calibri"
    st.font.size = Pt(10.5)
    title = doc.add_heading("Mounty-Python: Always look on the bright side of the board.", 0)
    title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    subtitle = doc.add_paragraph()
    subtitle.alignment = WD_ALIGN_PARAGRAPH.CENTER
    sub_run = subtitle.add_run("Architecture Documentation")
    sub_run.bold = True
    sub_run.font.size = Pt(16)
    doc.add_paragraph("Structure: arc42. Diagrams: C4 model (draw.io sources .drawio in this folder).")
    doc.add_paragraph("Sources: README.md, copilot instructions, AppHost.cs and the implementation.")
    for title, items in SECTIONS:
        doc.add_heading(title, 1)
        for item in items:
            kind = item[0]
            if kind == "p":
                doc.add_paragraph(item[1])
            elif kind == "h2":
                doc.add_heading(item[1], 2)
            elif kind == "b":
                for t in item[1]:
                    doc.add_paragraph(t, style="List Bullet")
            elif kind == "t":
                header, rows = item[1], item[2]
                tbl = doc.add_table(rows=1, cols=len(header))
                tbl.style = "Light Grid Accent 1"
                for i, h in enumerate(header):
                    tbl.rows[0].cells[i].text = h
                for r in rows:
                    cells = tbl.add_row().cells
                    for i, v in enumerate(r):
                        cells[i].text = v
                doc.add_paragraph()
            elif kind == "img":
                doc.add_picture(os.path.join(ROOT, item[1] + ".png"), width=Inches(6.3))
    doc.save(os.path.join(ROOT, "Architecture.docx"))


if __name__ == "__main__":
    for spec in DIAGRAMS:
        render_drawio(spec, os.path.join(ROOT, spec["file"] + ".drawio"))
        render_png(spec, os.path.join(ROOT, spec["file"] + ".png"))
    build_doc()
    print("done")
