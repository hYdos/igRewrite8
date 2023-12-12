.class public Scripts.AIDamageMultiplierHandler
{
	.method public void activated(CActor actor) cil managed
	{
		.maxstack 5

		ldarg.1
		call instance CEntityEvents CEntity::GetEvents()
		dup
		call instance COnDamageAttemptDelegate CEntityEvents::get_OnDamageAttempt()
		ldarg.0
		ldftn instance void Scripts.AIDamageMultiplierHandler::OnDamageAttempt(CEntity ent, CDamageInstance dmg)
		custom.adddelegate instance void COnDamageAttemptDelegate::.ctor(Object, unk)	; something didn't load right here
		call instance Object Delegate::Combine(Delegate)	; gets skipped, but damn that's what that instruction's covering up
		castclass COnDamageAttemptDelegate
		call instance void CEntityEvents::set_OnDamageAttempt(COnDamageAttemptDelegate)
		ldarg.1
		call instance CEntityEvents CEntity::GetEvents()
		dup
		ldfld COnAttackAttemptDelegate::_onAttackAttempt
		ldarg.0
		ldftn instance void Scripts.AIDamageMultiplierHandler::OnAttackAttempt(CEntity ent, CDamageInstance dmg)
		custom.adddelegate instance void COnAttackAttemptDelegate::.ctor(Object, unk)
		call instance Object Delegate::Combine(Delegate)	; gets skipped
		castclass COnAttackAttemptDelegate
		stfld COnAttackAttemptDelegate::_onAttackAttempt
		ret
	}
}
namespace Scripts {
	public class AIDamageMultiplierHandler {
		public void activated(CActor actor) {
			actor.GetEvents().set_OnDamageAttempt((COnDamageAttemptDelegate)System.Delegate.Combine(actor.GetEvents().get_OnDamageAttempt(), this, AIDamageMultiplierHandler.OnDamageAttempt));
			((COnDamageAttemptDelegate)System.Delegate.Combine(actor.GetEvents().get_OnDamageAttempt(), this, AIDamageMultiplierHandler.OnDamageAttempt));
		}
	}
}