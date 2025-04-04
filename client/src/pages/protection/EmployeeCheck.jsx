import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router";

export default function EmployeeCheck() {
    const [isAuthorized, setIsAuthorized] = useState(null);

    useEffect(() => {
        async function getRole() {
            const response = await fetch(`/api/login/role`, { credentials: "include" })
            const result = await response.json()

            if (response.ok && (result.role === "USER" || result.role === "ADMIN")) {
                setIsAuthorized(true);
            } else {
                setIsAuthorized(false);
            }
        }

        getRole()
    }, [])

    if (isAuthorized === null) {
        return <p>Laddar...</p>;
    }

    if (!isAuthorized) {
        return <Navigate to="/login" replace />;
    }

    return <Outlet />;
}
