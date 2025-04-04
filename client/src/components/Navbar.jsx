import { useContext } from "react";
import { NavLink } from "react-router";
import { GlobalContext } from "../GlobalContext.jsx";

export default function Navbar() {
    const { user, setUser } = useContext(GlobalContext)

    async function logout() {
        const response = await fetch("/api/login", {
            method: "delete",
            credentials: "include"
        })
        const result = await response.json()

        if (response.ok) {
            setUser(null)
        } else {
            alert(`Status: ${response.status}\n${result.message}`)
        }
    }

    return <nav>
        <NavLink to={"/"}>Home</NavLink>
        {
            user == null ?
                <>
                    <NavLink to={"/register"}>Register</NavLink>
                    <NavLink to={"/login"}>Login</NavLink>
                </>
                :
                <>
                    <NavLink to={"employee/chat/1"}>Employee Chat</NavLink>
                    <NavLink to={"/employee/issues"}>Issues</NavLink>
                    {
                        user.role == "ADMIN" ?
                            <>
                                <NavLink to={"/admin/employees"}>Employees</NavLink>
                                <NavLink to={"/admin/form/edit"}>Form subjects</NavLink>
                                <NavLink to={`/${user.company}/issueform`}>Veiw form</NavLink>
                            </>
                            :
                            null
                    }
                    <button onClick={logout}>Logout</button>
                </>
        }
    </nav>
}
