import { useNavigation, useRouter } from "simple-react-routing";
import { getAlunoById, salvarAluno } from "../../../services/alunoService";
import { useEffect, useReducer, useState } from "react";

export default function AlunoCadastroPage() {

    const { pathParams } = useRouter();
    //const [cep,setCep] = useState();
    const [cep, setCep] = useReducer(
        (oldValue, newValue) => {
            console.log(oldValue, newValue);

            if (newValue) {
                const apenasDigitos =
                    newValue.replace(/[^\d]/g, "").substr(-8);
                const valorMascarado = apenasDigitos.replace(/^(\d{5})(\d)/, "$1-$2")
                return valorMascarado;
            }

            return newValue?.toLowerCase();
        }, "");

    const { navigateTo } = useNavigation();

    const [current, setCurrent] = useState();

    const [erros, setErros] = useState([]);

    const submitForm = async (e) => {
        e.preventDefault();
        var formData = new FormData(e.target);
        const aluno = {
            id: pathParams["id"],
            nome: formData.get("nome"),
            cep: formData.get("cep"),
            email: formData.get("email"),
            dataNascimento: new Date(formData.get("data-nascimento")),
        }

        const response = await salvarAluno(aluno);
        if (response.status == 200) {
            navigateTo(e, "/alunos")
        }
        else {
            if (response.status == 422) {
                setErros(response.data);
            }
        }
    }

    useEffect(() => {
        if (pathParams["id"]) {
            getAlunoById(pathParams["id"])
                .then(response => {
                    if (response.status == 200) {
                        setCurrent(response.data);
                    }
                })
        }
        else {
            setCurrent({
                dataNascimento: new Date()
            });
        }
    }, [])

    return current && <>
        <h1>Cadastro de aluno</h1>

        <form className="column" onSubmit={submitForm}>
            <div className="column">
                <label>Nome:</label>
                <input required defaultValue={current?.nome} name="nome" type="text" />
            </div>
            <div className="column">
                <label>E-mail:</label>
                <input
                    onBlur={(e) => e.target.value = e.target.value.toLowerCase()}
                    defaultValue={current?.email} name="email" type="email" />
            </div>
            <div className="column">
                <label>CEP:</label>
                <input
                    value={cep}
                    onKeyDown={(e) => {
                        if (e.key.length == 1 && !e.key.match(/\d/)) {
                            e.preventDefault();
                        }
                    }}
                    onChange={(e) => setCep(e.target.value)}
                    pattern="^\d{5}-\d{3}$" defaultValue={current?.cep} name="cep" type="cep" />
            </div>
            <div className="column">
                <label>Data de nascimento:</label>
                {/* YYYY-MM-DD */}
                <input defaultValue={new Date(current?.dataNascimento).toISOString().split('T')[0]} name="data-nascimento" type="date" />
            </div>
            <div className="row-end">
                <div className="column">
                    {erros.map(e => <strong className="error">{e.propriedade}: {e.mensagem}</strong>)}
                </div>
                <button type="reset">Cancelar</button>
                <button type="submit">Salvar</button>
            </div>
        </form>
    </>;
}