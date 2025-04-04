import { useNavigate } from "react-router"

export default function Register() {
    const navigate = useNavigate()
    async function register(formData) {
        const response = await fetch(`/api/users/admin`, {
            method: "post",
            credentials: 'include',
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                email: formData.get("email"),
                password: formData.get("password"),
                username: formData.get("username"),
                company: formData.get("company")
            })
        })

        const result = await response.json()
        console.log(result)

        if (response.ok) {
            alert("Du har lyckats registrera dig!\nDu kommer bli hänvisad till Login-sidan.")
            navigate("/login")
        } else {
            alert(`Status: ${response.status}\n${result.message}`)
        }
    }

    return <form action={register}>
        <input type="email" name="email" placeholder="Email" required />
        <input type="password" name="password" placeholder="Password" required />
        <input type="text" name="username" placeholder="Username" required />
        <input type="text" name="company" placeholder="Company" required />
        <button type="submit">Skapa konto</button>
    </form>
}
