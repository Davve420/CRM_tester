import { createContext, useEffect, useState } from "react";

const GlobalContext = createContext()

function GlobalProvider({ children }) {
    const [user, setUser] = useState(null)

    async function getLogin() {
        const response = await fetch("/api/login", { credentials: "include" })
        const result = await response.json()

        if (response.ok) {
            setUser(result)
        } else {
            setUser(null)
        }
    }

    useEffect(() => {
        getLogin()
    }, [])

    return <GlobalContext.Provider value={{
        user, 
        setUser,
        getLogin
    }}>
        {children}
    </GlobalContext.Provider>

}

export { GlobalContext, GlobalProvider }