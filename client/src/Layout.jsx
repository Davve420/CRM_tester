import { Outlet } from "react-router";
import { useContext } from "react";
import Navbar from "./components/Navbar.jsx";
import { GlobalContext } from "./GlobalContext.jsx";

export default function Layout() {
    const { user } = useContext(GlobalContext)

    return <>
        <header>
            <Navbar />
            {user ?
                <h1 id="companyName">{user.company}</h1> : null}
        </header>
        <main>
            <Outlet />
        </main>
    </>
}
