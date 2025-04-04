import { useEffect, useState } from "react"
import { NavLink } from "react-router"

export default function Home() {
    const [companies, setCompanies] = useState([])

    async function getCompanies() {
        const response = await fetch(`/api/companies`, { credentials: "include" })
        const result = await response.json()

        if (response.ok) {
            setCompanies(result.companies)
        } else {
            alert(result.message)
        }
    }

    useEffect(() => {
        getCompanies()
    }, [])

    return <div id="startpage">
        <h2>All our partners</h2>
        <div id="companiesList">
            {
                companies.map(company => <NavLink key={company} to={`/${company}/issueform`}>{company}</NavLink>)
            }
        </div>
    </div>
}
