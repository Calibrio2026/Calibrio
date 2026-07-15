import { t as require_jsx_runtime } from "../index.js";
//#region app/error.tsx
var import_jsx_runtime = require_jsx_runtime();
function GlobalError() {
	const resetPreview = () => {
		localStorage.removeItem("calibrio-demo-user");
		localStorage.removeItem("calibrio-lab-assets");
		localStorage.removeItem("calibrio-lab-columns");
		localStorage.removeItem("calibration_field_order");
		window.location.reload();
	};
	return /* @__PURE__ */ (0, import_jsx_runtime.jsx)("main", {
		style: {
			minHeight: "100vh",
			display: "grid",
			placeItems: "center",
			fontFamily: "Arial,sans-serif",
			background: "#f7f9fc",
			color: "#1a304c"
		},
		children: /* @__PURE__ */ (0, import_jsx_runtime.jsxs)("section", {
			style: {
				width: "min(440px,calc(100% - 40px))",
				background: "#fff",
				padding: 32,
				borderRadius: 10,
				border: "1px solid #dfe7ef"
			},
			children: [
				/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
					style: {
						fontSize: 11,
						letterSpacing: ".1em",
						color: "#6e8da6",
						fontWeight: 700
					},
					children: "CALIBRIO PREVIEW"
				}),
				/* @__PURE__ */ (0, import_jsx_runtime.jsx)("h1", {
					style: {
						fontSize: 25,
						margin: "8px 0"
					},
					children: "Refresh the local workspace"
				}),
				/* @__PURE__ */ (0, import_jsx_runtime.jsx)("p", {
					style: {
						color: "#68798f",
						lineHeight: 1.5
					},
					children: "A saved local setting prevented this preview from loading. Resetting local preview data will reopen the blank workspace."
				}),
				/* @__PURE__ */ (0, import_jsx_runtime.jsx)("button", {
					type: "button",
					onClick: resetPreview,
					style: {
						marginTop: 12,
						border: 0,
						borderRadius: 6,
						background: "#167eae",
						color: "#fff",
						padding: "10px 14px",
						fontWeight: 700,
						cursor: "pointer"
					},
					children: "Reset local preview"
				})
			]
		})
	});
}
//#endregion
export { GlobalError as default };
