import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router";
import AccessDenied from "../AccessDenied.jsx";

export default function AdminCheck() {
    const [status, setStatus] = useState("loading");

    useEffect(() => {
        async function getRole() {
            const response = await fetch(`/api/login/role`, { credentials: "include" });
            const result = await response.json();

            if (response.ok) {
                if (result.role === "USER") {
                    setStatus("denied");
                } else {
                    setStatus("authorized");
                }
            } else {
                setStatus("unauthorized");
            }
        }

        getRole();
    }, []);

    if (status === "loading") {
        return <p>Laddar behörighet...</p>
    }
    
    if (status === "unauthorized") {
        return <Navigate to="/login" replace />
    }

    if (status === "denied") {
        return <AccessDenied />
    }

    return <Outlet />;
}